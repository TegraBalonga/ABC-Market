using ABC_Market_MVC.Models;
using ABC_Market_MVC.Models.Services.Business;
using Microsoft.Ajax.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ABC_Market_MVC.Controllers
{
    public class HomeController : Controller
    {

        static CloudBlobClient blobClient;
        const string blobContainerName = "imagecontainer";
        static CloudBlobContainer blobContainer;
        private readonly IConfiguration _configuration;


        [HttpGet]
        public ActionResult LoginPage()
        {

            return View();

        }

        [HttpPost]
        public ActionResult LoginPage(AdminLogin admin)
        {
            SecurityService securityService = new SecurityService();

            bool success = securityService.Authenticate(admin);

            if ((admin.Username != null) || (admin.Password != null) || (admin.Username != "")
                || (admin.Password != ""))
            {
                if (success)
                {
                   
                    return RedirectToAction("MarketPage");

                }
                else
                {

                    ViewBag.error = "Username or password incorrect";
                   // return RedirectToAction("MarketPage");
                     return View("LoginPage");
                }
            }
            else
            {
                ViewBag.error = "Fill in all the fields";
                //return RedirectToAction("MarketPage");
                 return View("LoginPage");
            }



        }



       


        public HomeController() { }

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public async Task<ActionResult> MarketPlace()
        public async Task<ActionResult> MarketPage()
        {
            try
            {
                string storageConnectionString = _configuration.GetConnectionString("StorageConnectionString");

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);


                blobClient = storageAccount.CreateCloudBlobClient();
                blobContainer = blobClient.GetContainerReference(blobContainerName);
                await blobContainer.CreateIfNotExistsAsync();


                await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });


                List<Uri> allBlobs = new List<Uri>();
                BlobContinuationToken blobContinuationToken = null;

                do
                {

                    var response = await blobContainer.ListBlobsSegmentedAsync(blobContinuationToken);

                    foreach (IListBlobItem blob in response.Results)
                    {
                        if (blob.GetType() == typeof(CloudBlockBlob))
                            allBlobs.Add(blob.Uri);
                    }

                    blobContinuationToken = response.ContinuationToken;

                } while (blobContinuationToken != null);



                return View(allBlobs);

            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }


        [HttpPost]
        public async Task<ActionResult> UploadAsync()
        {
            try
            {
                //var request = await HttpContext.Request.ReadFormAsync();
                HttpFileCollectionBase files = Request.Files;
                int fileCount = files.Count;

                if (fileCount > 0)
                {
                    for (int i = 0; i < fileCount; i++)
                    {
                        var blob = blobContainer.GetBlockBlobReference(GetRandomBlobName(files[i].FileName));

                        using (var stream = files[i].InputStream)
                        {
                            await blob.UploadFromStreamAsync(stream);
                        }
                    }
                }
                return RedirectToAction("MarketPage");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }



        [HttpPost]
        public async Task<ActionResult> DeleteImage(string name)
        {
            try
            {
                Uri uri = new Uri(name);
                string filename = Path.GetFileName(uri.LocalPath);

                var blob = blobContainer.GetBlockBlobReference(filename);
                await blob.DeleteIfExistsAsync();

                return RedirectToAction("MarketPage");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

      

        [HttpPost]
        public async Task<ActionResult> DeleteAll()
        {

            BlobContinuationToken blobContinuationToken = null;
            try
            {
                do
                {

                    var response = await blobContainer.ListBlobsSegmentedAsync(blobContinuationToken);

                    foreach (IListBlobItem blob in response.Results)
                    {
                        if (blob.GetType() == typeof(CloudBlockBlob))
                            await ((CloudBlockBlob)blob).DeleteIfExistsAsync();
                    }

                    blobContinuationToken = response.ContinuationToken;

                } while (blobContinuationToken != null);

                

                return RedirectToAction("MarketPage");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

     
        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
    }
}
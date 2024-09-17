using ABC_Market_MVC.Models.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ABC_Market_MVC.Models.Services.Business
{
    public class SecurityService
    {
        SecurityDAO daoService = new SecurityDAO();

        public bool Authenticate(AdminLogin admin)
        {
            return daoService.FindByUser(admin);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace ABC_Market_MVC.Models.Services.Data
{
    public class SecurityDAO
    {
        string connectionString = @"Data Source=DESKTOP-9A2QO06\MSSQL;Initial Catalog=ABC_MARKET;Integrated Security=True";

        internal bool FindByUser(AdminLogin admin)
        {
            bool success = false;

            string query = "SELECT * FROM TBL_ADMIN WHERE USERNAME = @USER AND PASSWORD = @PASS";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.Add("@USER", System.Data.SqlDbType.VarChar, 50).Value = admin.Username;
                command.Parameters.Add("@PASS", System.Data.SqlDbType.VarChar, 50).Value = admin.Password;


                try
                {
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        success = true;
                    }else
                    {
                        success = false;
                    }

                    reader.Close();
                }catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return success;
        }
    }
}
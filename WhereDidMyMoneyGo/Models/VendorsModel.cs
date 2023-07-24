using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WhereDidMyMoneyGo.Models
{
    public class VendorsModel
    {
        public VendorsModel()
        {
            //When this Model is call, create a database object to use for calling the database with UserDapperRepo actions
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

            string connString = config.GetConnectionString("DefaultConnection");
            IDbConnection conn = new MySqlConnection(connString);

            RepoVen = new VendorsDapperRepo(conn);
        }

        public VendorsDapperRepo RepoVen { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public int UserId { get; set; }
        public IEnumerable<SelectListItem> AllVendors { get; set; }

        //List all default Vendors and Vendors entered by user
        public void GetAllVendors(int userId)
        {
            var vendors = RepoVen.GetDefaultAndUserVendors(userId);
            AllVendors = vendors.Select(x => new SelectListItem() { Text = x.VendorName.ToString(), Value = x.UserId.ToString() });
        }
    }
}

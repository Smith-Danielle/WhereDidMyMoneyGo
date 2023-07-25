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
        public string DropDownVenOption { get; set; } //Vendor option selected from view, this will also determine if the vendor chosen is new

        //List all default Vendors and Vendors entered by user
        public void GetAllVendors(int userId)
        {
            var vendors = RepoVen.GetDefaultAndUserVendors(userId);
            VendorsTable blank = new VendorsTable() { VendorName = "" }; //This is for View to have an empty option
            VendorsTable addNew = new VendorsTable() { VendorName = "*ADD NEW VENDOR*" }; //This is for View to have option to add new vendor
            var listVendors = vendors.ToList();
            listVendors.Insert(0, blank);
            listVendors.Insert(1, addNew);
            AllVendors = listVendors.Select(x => new SelectListItem() { Text = x.VendorName.ToString(), Value = x.VendorName.ToString() });
        }
    }
}

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
        public IEnumerable<SelectListItem> AllVendorsSelect { get; set; } //Formatted for dropdown on view, trans entry
        public string DropDownVenOption { get; set; } //Vendor option selected from view, this will also determine if the vendor chosen is new
        public IEnumerable<VendorsTable> AllVendorsInfo { get; set; }

        //List all Vendors entered by user as SelectListItem
        public void GetAllVendorsSelect(int userId)
        {
            var vendors = RepoVen.GetUserVendors(userId);
            VendorsTable blank = new VendorsTable() { VendorName = "" }; //This is for View to have an empty option
            VendorsTable addNew = new VendorsTable() { VendorName = "*ADD NEW VENDOR*" }; //This is for View to have option to add new vendor
            var listVendors = vendors.Where(x => x.VendorName != "User Adjustment").ToList();
            listVendors.Insert(0, blank);
            listVendors.Insert(1, addNew);
            AllVendorsSelect = listVendors.Select(x => new SelectListItem() { Text = x.VendorName.ToString(), Value = x.VendorName.ToString() });
        }

        //List all Vendors entered by users normal, all vendor info
        public void GetVendors(int userId)
        {
            AllVendorsInfo = RepoVen.GetUserVendors(userId);
        }

        //Add new inputted Vendor to database
        public void AddNewVendor(int userId, string vendorName)
        {
            RepoVen.InsertNewVendor(userId, vendorName);
        }

        //Delete user selected vendor
        public void DeleteVendor(int userId, string vendorName)
        {
            RepoVen.DeleteUserVendor(userId, vendorName);
        }

        //Add default vendor options for new users
        public void AddDefaultVendors(int userId)
        {
            RepoVen.InsertNewUserDefaultVendors(userId);
        }
    }
}

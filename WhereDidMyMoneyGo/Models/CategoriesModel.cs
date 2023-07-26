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
    public class CategoriesModel
    {
        public CategoriesModel()
        {
            //When this Model is call, create a database object to use for calling the database with UserDapperRepo actions
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

            string connString = config.GetConnectionString("DefaultConnection");
            IDbConnection conn = new MySqlConnection(connString);

            RepoCat = new CategoriesDapperRepo(conn);
        }

        public CategoriesDapperRepo RepoCat { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int UserId { get; set; }
        public IEnumerable<SelectListItem> AllCategories { get; set; }
        public IEnumerable<SelectListItem> DropDownCatOption { get; set; } //Category option selected from view, this will also determine if the category chosen is new

        //List all default Vendors and Vendors entered by user
        public void GetAllCategories(int userId)
        {
            var categories = RepoCat.GetDefaultAndUserCats(userId);
            CategoriesTable blank = new CategoriesTable() { CategoryName = "" }; //This is for View to have an empty option
            CategoriesTable addNew = new CategoriesTable() { CategoryName = "*ADD NEW CATEGORY*" }; //This is for View to have option to add new category
            var listCategories = categories.ToList();
            listCategories.Insert(0, blank);
            listCategories.Insert(1, addNew);
            AllCategories = listCategories.Select(x => new SelectListItem() { Text = x.CategoryName.ToString(), Value = x.CategoryName.ToString() });
        }
    }
}

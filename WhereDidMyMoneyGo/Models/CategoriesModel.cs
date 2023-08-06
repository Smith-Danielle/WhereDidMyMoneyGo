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
        public IEnumerable<SelectListItem> AllCategoriesSelect { get; set; } //Formatted for dropdown on view, trans entry
        public string DropDownCatOption { get; set; } //Category option selected from view, this will also determine if the category chosen is new
        public IEnumerable<CategoriesTable> AllCategoriesInfo { get; set; }

        //List all Categories entered by user as SelectListItem
        public void GetAllCategoriesSelect(int userId)
        {
            var categories = RepoCat.GetUserCats(userId);
            CategoriesTable blank = new CategoriesTable() { CategoryName = "" }; //This is for View to have an empty option
            CategoriesTable addNew = new CategoriesTable() { CategoryName = "*ADD NEW CATEGORY*" }; //This is for View to have option to add new category
            var listCategories = categories.Where(x => x.CategoryName != "Balance Adjustment").ToList();
            CategoriesTable all = new CategoriesTable() { CategoryName = "ALL CATEGORIES" }; //This is for reports view to run report including all categories
            listCategories.Insert(0, blank);
            listCategories.Insert(1, addNew);
            listCategories.Insert(2, all);
            AllCategoriesSelect = listCategories.Select(x => new SelectListItem() { Text = x.CategoryName.ToString(), Value = x.CategoryName.ToString() });
        }

        //List all Categories entered by users normal, all vendor info
        public void GetCategories(int userId)
        {
            AllCategoriesInfo = RepoCat.GetUserCats(userId);
        }

        //Add new inputted Category to database
        public void AddNewCategory(int userId, string categoryName)
        {
            RepoCat.InsertNewCategory(userId, categoryName);
        }

        //Delete user selected category
        public void DeleteCategory(int userId, string categoryName)
        {
            RepoCat.DeleteUserCategory(userId, categoryName);
        }

        //Add default category options for new users
        public void AddDefaultCats (int userId)
        {
            RepoCat.InsertNewUserDefaultCats(userId);
        }
    }
}

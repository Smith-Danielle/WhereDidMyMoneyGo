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
        public int CategoryName { get; set; }
        public int CategoryType { get; set; } //Enum in database. Values: Revenue, Expense, Adjustment 
        public int UserId { get; set; }
        public IEnumerable<SelectListItem> AllCategories { get; set; }

        //List all default Vendors and Vendors entered by user
        public void GetAllCategories(int userId)
        {
            var categories = RepoCat.GetDefaultAndUserCats(userId);
            AllCategories = categories.Select(x => new SelectListItem() { Text = x.CategoryName.ToString(), Value = x.CategoryId.ToString() });
        }
    }
}

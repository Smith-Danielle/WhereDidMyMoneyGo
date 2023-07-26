using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WhereDidMyMoneyGo.Models;

namespace WhereDidMyMoneyGo
{
    public class OverviewViewModel
    {
        public OverviewViewModel()
        {
        }

        public UsersModel OverUsersModel { get; set; }
        public TransactionsModel OverTransactionsModel { get; set; }
        public VendorsModel OverVendorsModel { get; set; }
        public CategoriesModel OverCategoriesModel { get; set; }

        public List<string> Messages { get; set; }
    }
}

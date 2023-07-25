﻿using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WhereDidMyMoneyGo.Models
{
    public class TransactionsModel
    {
        public TransactionsModel()
        {
            //When this Model is call, create a database object to use for calling the database with TransactionDapperRepo actions
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

            string connString = config.GetConnectionString("DefaultConnection");
            IDbConnection conn = new MySqlConnection(connString);

            RepoTrans = new TransactionsDapperRepo(conn);

            List<string> transtypes = new List<string>() {"", "Revenue", "Expense" };
            TransTypeOptions = transtypes.Select(x => new SelectListItem() { Text = x, Value = x });
        }

        public TransactionsDapperRepo RepoTrans { get; set; }
        public int TransactionId { get; set; }
        public string TransactionDate { get; set; } //Before sending to repo, TransactionDate.ToString("yyyy-MM-dd");
        public string TransactionType { get; set; } //Enum in database. Values: Revenue, Expense, Adjustment 
        public string TransactionAmount { get; set; } //In database this is a double. Will start out as string here, then validate user input, and convert before sending to database
        public int VendorId { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string VendorName { get; set; } //from Vendor table, join done in dapper repo, not needed for a transaction
        public string CategoryName { get; set; } //from Category table, join done in dapper repo, not needed for a transaction
        public IEnumerable<TransactionsTable> TopFiveTransactions { get; set; }
        public IEnumerable<SelectListItem> TransTypeOptions { get; set; }


        //List top 5 transactions
        public void TopTrans(int userId)
        {
            var top = RepoTrans.GetUserTrans(userId);
            TopFiveTransactions = top.Count() > 5 ? top.Take(5) : top;
        }
    }
}

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

            List<string> adjusttypes = new List<string>() { "", "Adjustment Increase", "Adjustment Decrease" };
            AdjustTypeOptions = adjusttypes.Select(x => new SelectListItem() { Text = x, Value = x });
        }

        public TransactionsDapperRepo RepoTrans { get; set; }
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; } //Before sending to repo, TransactionDate.ToString("yyyy-MM-dd");
        public string TransactionType { get; set; } //Enum in database. Values: Revenue, Expense, Adjustment Increase, Adjustment Decrease 
        public string TransactionAmount { get; set; } //In database this is a double. Will start out as string here, then validate user input, and convert before sending to database
        public int VendorId { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string VendorName { get; set; } //from Vendor table, join done in dapper repo, not needed for a transaction
        public string CategoryName { get; set; } //from Category table, join done in dapper repo, not needed for a transaction
        public IEnumerable<TransactionsTable> TopFiveTransactions { get; set; } //for overview home page
        public IEnumerable<SelectListItem> TransTypeOptions { get; set; } //Formatted for dropdown on view, trans entry
        public string DropDownTypeSelection { get; set; } //Type selected from view, will be assigned to final Transaction type
        public IEnumerable<SelectListItem> AdjustTypeOptions { get; set; } //Formatted for dropdown on view, adjust balance
        public List<DataPoint> DataPointsType { get; set; } //for overview page type pie chart
        public List<DataPoint> DataPointsMonthly { get; set; } //for overview page monthly column chart


        //List top 5 transactions
        public void OverviewInfo(int userId)
        {
            var trans = RepoTrans.GetUserTrans(userId);

            //Top 5 Transactions
            TopFiveTransactions = trans.Count() > 5 ? trans.Take(5) : trans;

            //Type Pie Chart
            double total = trans.Select(x => x.TransactionAmount).Sum();
            var types = trans.GroupBy(x => x.TransactionType).Select(x => x.Key).OrderByDescending(x => x);
            var stats = types.Select(x => new { Type = x, Percent = Math.Round((trans.Where(y => y.TransactionType == x).Select(y => y.TransactionAmount).Sum() / total) * 100, 2)});
            DataPointsType = new List<DataPoint>();
            foreach (var item in stats)
            {
                DataPoint temp = new DataPoint(item.Type, item.Percent);
                DataPointsType.Add(temp);
            }

            //Month to Date spending
            var currentMonth = trans.Where(x => Convert.ToDateTime(x.TransactionDate).Month == DateTime.Now.Month && Convert.ToDateTime(x.TransactionDate).Year == DateTime.Now.Year).GroupBy(x => x.TransactionDate).Select(x => x.Key).OrderBy(x => x);
            var monthlyTotals = currentMonth.Select(x => new { Date = Convert.ToDateTime(x).Ticks.ToString(), Total = Math.Round(trans.Where(y => y.TransactionDate == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() -
                                                                                                             trans.Where(y => y.TransactionDate == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2) });
            DataPointsMonthly = new List<DataPoint>();
            foreach (var item in monthlyTotals)
            {
                DataPoint temp = new DataPoint(item.Date, item.Total);
                DataPointsMonthly.Add(temp);
            }

        }

        //Add new Transaction to database 
        public void AddNewTrans(int userId, int venId, int catId, string date, string type, double amount)
        {
            RepoTrans.InsertNewTrans(userId, venId, catId, date, type, amount);
        }

    }
}

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

            List<string> transtypes = new List<string>() { "", "Revenue", "Expense" };
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
        public List<DataPointCircular> DataPointsType { get; set; } //for overview page type pie chart
        public List<DataPoint> DataPointsMonthly { get; set; } //for overview page monthly column chart
        public List<DataPoint> DataPointsYearly { get; set; } //for overview page yearly column chart
        public List<DataPointCircular> DataPointsVendor { get; set; } // for overview page vendor bar chart
        public List<DataPointCircular> DataPointsCategory { get; set; } // for overview page category bar chart
        public IEnumerable<TransactionsTable> AllTransActions { get; set; } // all user transactions

        //List top 5 transactions
        public void OverviewInfo(int userId)
        {
            var trans = RepoTrans.GetUserTrans(userId);

            //Top 5 Transactions
            TopFiveTransactions = trans.Count() > 5 ? trans.Take(5) : trans;

            //Current Year Activity Type Pie Chart
            var types = trans.GroupBy(x => x.TransactionType).Select(x => x.Key).OrderByDescending(x => x);
            var stats = types.Select(x => new { Type = x, Total = Math.Round(trans.Where(y => y.TransactionType == x && Convert.ToDateTime(y.TransactionDate).Year == DateTime.Now.Year).Select(y => y.TransactionAmount).Sum(), 2) });
            DataPointsType = new List<DataPointCircular>();

            foreach (var item in stats)
            {
                DataPointCircular temp = new DataPointCircular(item.Total, item.Type);
                DataPointsType.Add(temp);
            }

            //Monthly spending
            var currentMonth = trans.Where(x => Convert.ToDateTime(x.TransactionDate).Month == DateTime.Now.Month && Convert.ToDateTime(x.TransactionDate).Year == DateTime.Now.Year).GroupBy(x => x.TransactionDate).Select(x => x.Key).OrderBy(x => x);
            var dailyTotals = currentMonth.Select(x => new
            {
                Day = Convert.ToDateTime(x).Day,
                Total = Math.Round(trans.Where(y => y.TransactionDate == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() -
                                                                                                                 trans.Where(y => y.TransactionDate == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
            });
            DataPointsMonthly = new List<DataPoint>();
            var daysInCurrentMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            for (int i = 1; i < daysInCurrentMonth + 1; i++)
            {
                if (dailyTotals.Where(x => x.Day == i).Any())
                {
                    DataPoint temp = new DataPoint(dailyTotals.Where(x => x.Day == i).Select(x => x.Total).First(), i.ToString());
                    DataPointsMonthly.Add(temp);
                }
                else
                {
                    DataPoint temp = new DataPoint(0, i.ToString());
                    DataPointsMonthly.Add(temp);
                }
            }

            //Yearly Spending
            var currentYear = trans.Where(x => Convert.ToDateTime(x.TransactionDate).Year == DateTime.Now.Year).GroupBy(x => Convert.ToDateTime(x.TransactionDate).Month).Select(x => x.Key).OrderBy(x => x);
            var monthlyTotals = currentYear.Select(x => new
            {
                Month = x,
                Total = Math.Round(trans.Where(y => Convert.ToDateTime(y.TransactionDate).Month == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() -
                                                                                           trans.Where(y => Convert.ToDateTime(y.TransactionDate).Month == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
            });
            DataPointsYearly = new List<DataPoint>();
            List<string> monthNames = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            for (int i = 1; i < 13; i++)
            {
                if (monthlyTotals.Where(x => x.Month == i).Any())
                {
                    DataPoint temp = new DataPoint(monthlyTotals.Where(x => x.Month == i).Select(x => x.Total).First(), monthNames[i - 1]);
                    DataPointsYearly.Add(temp);
                }
                else
                {
                    DataPoint temp = new DataPoint(0, monthNames[i - 1]);
                    DataPointsYearly.Add(temp);
                }
            }

            //Vendor Spending Most Used Current Year
            var vendorsAll = trans.Where(x => x.VendorName != "User Adjustment" && x.TransactionType == "Expense" && Convert.ToDateTime(x.TransactionDate).Year == DateTime.Now.Year);
            var groupedVendors = vendorsAll.GroupBy(x => x.VendorName).Select(x => x.Key);
            var topVendors = groupedVendors.Select(x => new { Name = x, Count = vendorsAll.Where(y => y.VendorName == x).Count() }).OrderByDescending(x => x.Count).ThenByDescending(x => vendorsAll.Where(y => y.VendorName == x.Name).Select(y => y.TransactionAmount).Sum()).ThenBy(x => x.Name).Select(x => x.Name).Take(5);
            var vendorTotals = topVendors.Select(x => new { Vendor = x, Total = Math.Round(vendorsAll.Where(y => y.VendorName == x).Select(y => y.TransactionAmount).Sum(), 2) }).OrderBy(x => x.Vendor).ToList();

            DataPointsVendor = new List<DataPointCircular>();
            for (int i = 0; i < vendorTotals.Count; i++)
            {
                DataPointCircular temp = new DataPointCircular(vendorTotals[i].Total, vendorTotals[i].Vendor);
                DataPointsVendor.Add(temp);
            }

            //Category Spending Most Used Current Year
            var categoryAll = trans.Where(x => x.CategoryName != "Balance Adjustment" && x.TransactionType == "Expense" && Convert.ToDateTime(x.TransactionDate).Year == DateTime.Now.Year);
            var groupedCategories = categoryAll.GroupBy(x => x.CategoryName).Select(x => x.Key);
            var topCategories = groupedCategories.Select(x => new { Name = x, Count = categoryAll.Where(y => y.CategoryName == x).Count() }).OrderByDescending(x => x.Count).ThenByDescending(x => categoryAll.Where(y => y.CategoryName == x.Name).Select(y => y.TransactionAmount).Sum()).ThenBy(x => x.Name).Select(x => x.Name).Take(5);
            var categoryTotals = topCategories.Select(x => new { Category = x, Total = Math.Round(categoryAll.Where(y => y.CategoryName == x).Select(y => y.TransactionAmount).Sum(), 2) }).OrderBy(x => x.Category).ToList();

            DataPointsCategory = new List<DataPointCircular>();
            for (int i = 0; i < categoryTotals.Count; i++)
            {
                DataPointCircular temp = new DataPointCircular(categoryTotals[i].Total, categoryTotals[i].Category);
                DataPointsCategory.Add(temp);
            }

        }

        //Add new Transaction to database 
        public void AddNewTrans(int userId, int venId, int catId, string date, string type, double amount)
        {
            RepoTrans.InsertNewTrans(userId, venId, catId, date, type, amount);
        }

        //Get all User Transactions
        public void GetAllUserTrans(int userId)
        {
            AllTransActions = RepoTrans.GetUserTrans(userId);
        }
    }
}

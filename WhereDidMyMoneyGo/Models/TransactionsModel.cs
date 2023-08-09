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

            List<string> reporttypes = new List<string>() { "", "ALL ACTIVITY TYPES", "Revenue", "Expense", "Adjustment Increase", "Adjustment Decrease", "Revenue & Expense", "Adjustment Increase & Adjustment Decrease" };
            ReportTypeOptions = reporttypes.Select(x => new SelectListItem() { Text = x, Value = x });

            List<string> reportgroups = new List<string>() { "", "NONE", "By Day", "By Month", "By Year" };
            ReportGroupOptions = reportgroups.Select(x => new SelectListItem() { Text = x, Value = x });
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
        public DateTime StartDateEntry { get; set; } //for reporting module
        public DateTime EndDateEntry { get; set; } //for reporting module
        public DateTime StartDateVendor { get; set; } //for reporting module
        public DateTime EndDateVendor { get; set; } //for reporting module
        public DateTime StartDateCategory { get; set; } //for reporting module
        public DateTime EndDateCategory { get; set; } //for reporting module
        public DateTime StartDateType { get; set; } //for reporting module
        public DateTime EndDateType { get; set; } //for reporting module
        public IEnumerable<SelectListItem> ReportTypeOptions { get; set; } //Formatted for Type on dropdown on view, reports
        public IEnumerable<SelectListItem> ReportGroupOptions { get; set; } //Formatted for Group dropdown on view, reports
        public string DropDownGroupSelection { get; set; } //Group selected from view

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

        //Activty Entry Report for Report Module
        public void EntryReport(int userId, string group, DateTime start, DateTime end)
        {
            var trans = RepoTrans.GetUserTrans(userId).Where(x => Convert.ToDateTime(x.TransactionDate) >= start && Convert.ToDateTime(x.TransactionDate) <= end).OrderBy(x => Convert.ToDateTime(x.TransactionDate)).ThenBy(x => x.VendorName).ThenBy(x => x.CategoryName).ThenBy(x => x.TransactionType).ThenBy(x => x.TransactionAmount);
            var transTotal = Math.Round(trans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Adjustment Increase").Select(x => x.TransactionAmount).Sum() - trans.Where(x => x.TransactionType == "Expense" || x.TransactionType == "Adjustment Decrease").Select(x => x.TransactionAmount).Sum(), 2);
            var allTrans = trans.ToList();

            //if grouping by "NONE", just show allTrans, total will be added at the end

            if (group == "By Day")
            {
                var days = trans.GroupBy(x => x.TransactionDate).Select(x => x.Key).Select(x => new
                {
                    Date = x,
                    Count = trans.Where(y => y.TransactionDate == x).Count(),
                    Total = Math.Round(trans.Where(y => y.TransactionDate == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() - trans.Where(y => y.TransactionDate == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
                }).ToList();
                var transDates = trans.Select(x => x.TransactionDate).ToList();
                for (int i = 0; i < days.Count(); i++)
                {
                    transDates.Insert(transDates.IndexOf(days[i].Date) + days[i].Count, $"({i})");
                    allTrans.Insert(transDates.IndexOf($"({i})"), new TransactionsTable { TransactionAmount = days[i].Total });
                }
            }

            if (group == "By Month")
            {
                var months = trans.GroupBy(x => new { Convert.ToDateTime(x.TransactionDate).Month, Convert.ToDateTime(x.TransactionDate).Year }).Select(x => new { x.Key.Month, x.Key.Year }).Select(x => new
                {
                    DateMonth = x.Month,
                    DateYear = x.Year,
                    Count = trans.Where(y => Convert.ToDateTime(y.TransactionDate).Month == x.Month && Convert.ToDateTime(y.TransactionDate).Year == x.Year).Count(),
                    Total = Math.Round(trans.Where(y => Convert.ToDateTime(y.TransactionDate).Month == x.Month && Convert.ToDateTime(y.TransactionDate).Year == x.Year).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() - trans.Where(y => Convert.ToDateTime(y.TransactionDate).Month == x.Month && Convert.ToDateTime(y.TransactionDate).Year == x.Year).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
                }).OrderBy(x => x.DateYear).ThenBy(x => x.DateMonth).ToList();
                var transDates = trans.Select(x => x.TransactionDate).ToList();
                int count = 0;
                for (int i = 0; i < months.Count(); i++)
                {
                    transDates.Insert(count + months[i].Count, $"({i})");
                    count += months[i].Count + 1;
                    allTrans.Insert(transDates.IndexOf($"({i})"), new TransactionsTable { TransactionAmount = months[i].Total });
                }
            }

            if (group == "By Year")
            {
                var years = trans.GroupBy(x => Convert.ToDateTime(x.TransactionDate).Year).Select(x => x.Key).Select(x => new
                {
                    Year = x,
                    Count = trans.Where(y => Convert.ToDateTime(y.TransactionDate).Year == x).Count(),
                    Total = Math.Round(trans.Where(y => Convert.ToDateTime(y.TransactionDate).Year == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() - trans.Where(y => Convert.ToDateTime(y.TransactionDate).Year == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
                }).ToList();
                var transDates = trans.Select(x => x.TransactionDate).ToList();
                int count = 0;
                for (int i = 0; i < years.Count(); i++)
                {
                    transDates.Insert(count + years[i].Count, $"({i})");
                    count += years[i].Count + 1;
                    allTrans.Insert(transDates.IndexOf($"({i})"), new TransactionsTable { TransactionAmount = years[i].Total });
                }
            }

            allTrans.Add(new TransactionsTable { TransactionDate = "Report Total", TransactionAmount = transTotal });
            AllTransActions = allTrans.Select(x => x);
        }

        //Vendor Report for Report Module
        public void VendorReport(int userId, string vendor, DateTime start, DateTime end)
        {
            var trans = RepoTrans.GetUserTrans(userId).Where(x => Convert.ToDateTime(x.TransactionDate) >= start && Convert.ToDateTime(x.TransactionDate) <= end && x.VendorName != "User Adjustment").OrderBy(x => x.VendorName).ThenBy(x => Convert.ToDateTime(x.TransactionDate)).ThenBy(x => x.CategoryName).ThenBy(x => x.TransactionType).ThenBy(x => x.TransactionAmount);
            
            if (vendor == "ALL VENDORS")
            {
                var vendors = trans.GroupBy(x => x.VendorName).Select(x => x.Key).Select(x => new
                {
                    Vendor = x,
                    Count = trans.Where(y => y.VendorName == x).Count(),
                    Total = Math.Round(trans.Where(y => y.VendorName == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() - trans.Where(y => y.VendorName == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
                }).ToList();
                var allTrans = trans.ToList();
                var transDates = trans.Select(x => x.TransactionDate).ToList();
                int count = 0;
                for (int i = 0; i < vendors.Count(); i++)
                {
                    transDates.Insert(count + vendors[i].Count, $"({i})");
                    count += vendors[i].Count + 1;
                    allTrans.Insert(transDates.IndexOf($"({i})"), new TransactionsTable { TransactionAmount = vendors[i].Total });
                }

                var transTotal = Math.Round(trans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Adjustment Increase").Select(x => x.TransactionAmount).Sum() - trans.Where(x => x.TransactionType == "Expense" || x.TransactionType == "Adjustment Decrease").Select(x => x.TransactionAmount).Sum(), 2);
                allTrans.Add(new TransactionsTable { TransactionDate = "Report Total", TransactionAmount = transTotal });
                AllTransActions = allTrans.Select(x => x);
            }

            else
            {
                var specificTrans = trans.Where(x => x.VendorName == vendor).ToList();
                var specificTotal = Math.Round(specificTrans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Adjustment Increase").Select(x => x.TransactionAmount).Sum() - specificTrans.Where(x => x.TransactionType == "Expense" || x.TransactionType == "Adjustment Decrease").Select(x => x.TransactionAmount).Sum(), 2);
                specificTrans.Add(new TransactionsTable { TransactionDate = "Report Total", TransactionAmount = specificTotal });
                AllTransActions = specificTrans.Select(x => x);
            }
        }

        //Entry Category for Report Module
        public void CategoryReport(int userId, string category, DateTime start, DateTime end)
        {
            var trans = RepoTrans.GetUserTrans(userId).Where(x => Convert.ToDateTime(x.TransactionDate) >= start && Convert.ToDateTime(x.TransactionDate) <= end && x.CategoryName != "Balance Adjustment").OrderBy(x => x.CategoryName).ThenBy(x => Convert.ToDateTime(x.TransactionDate)).ThenBy(x => x.VendorName).ThenBy(x => x.TransactionType).ThenBy(x => x.TransactionAmount);

            if (category == "ALL CATEGORIES")
            {
                var categories = trans.GroupBy(x => x.CategoryName).Select(x => x.Key).Select(x => new
                {
                    Category = x,
                    Count = trans.Where(y => y.CategoryName == x).Count(),
                    Total = Math.Round(trans.Where(y => y.CategoryName == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() - trans.Where(y => y.CategoryName == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
                }).ToList();
                var allTrans = trans.ToList();
                var transDates = trans.Select(x => x.TransactionDate).ToList();
                int count = 0;
                for (int i = 0; i < categories.Count(); i++)
                {
                    transDates.Insert(count + categories[i].Count, $"({i})");
                    count += categories[i].Count + 1;
                    allTrans.Insert(transDates.IndexOf($"({i})"), new TransactionsTable { TransactionAmount = categories[i].Total });
                }

                var transTotal = Math.Round(trans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Adjustment Increase").Select(x => x.TransactionAmount).Sum() - trans.Where(x => x.TransactionType == "Expense" || x.TransactionType == "Adjustment Decrease").Select(x => x.TransactionAmount).Sum(), 2);
                allTrans.Add(new TransactionsTable { TransactionDate = "Report Total", TransactionAmount = transTotal });
                AllTransActions = allTrans.Select(x => x);
            }

            else
            {
                var specificTrans = trans.Where(x => x.CategoryName == category).ToList();
                var specificTotal = Math.Round(specificTrans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Adjustment Increase").Select(x => x.TransactionAmount).Sum() - specificTrans.Where(x => x.TransactionType == "Expense" || x.TransactionType == "Adjustment Decrease").Select(x => x.TransactionAmount).Sum(), 2);
                specificTrans.Add(new TransactionsTable { TransactionDate = "Report Total", TransactionAmount = specificTotal });
                AllTransActions = specificTrans.Select(x => x);
            }
        }

        //Activty Type Report for Report Module
        public void TypeReport(int userId, string type, DateTime start, DateTime end)
        {
            var trans = RepoTrans.GetUserTrans(userId).Where(x => Convert.ToDateTime(x.TransactionDate) >= start && Convert.ToDateTime(x.TransactionDate) <= end && x.TransactionType != "Balance Adjustment").OrderBy(x => x.TransactionType).ThenBy(x => Convert.ToDateTime(x.TransactionDate)).ThenBy(x => x.VendorName).ThenBy(x => x.CategoryName).ThenBy(x => x.TransactionAmount);

            if (type == "ALL ACTIVITY TYPES")
            {
                var types = trans.GroupBy(x => x.TransactionType).Select(x => x.Key).Select(x => new
                {
                    Type = x,
                    Count = trans.Where(y => y.TransactionType == x).Count(),
                    Total = Math.Round(trans.Where(y => y.TransactionType == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() - trans.Where(y => y.TransactionType == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
                }).ToList();
                var allTrans = trans.ToList();
                var transDates = trans.Select(x => x.TransactionDate).ToList();
                int count = 0;
                for (int i = 0; i < types.Count(); i++)
                {
                    transDates.Insert(count + types[i].Count, $"({i})");
                    count += types[i].Count + 1;
                    allTrans.Insert(transDates.IndexOf($"({i})"), new TransactionsTable { TransactionAmount = types[i].Total });
                }

                var transTotal = Math.Round(trans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Adjustment Increase").Select(x => x.TransactionAmount).Sum() - trans.Where(x => x.TransactionType == "Expense" || x.TransactionType == "Adjustment Decrease").Select(x => x.TransactionAmount).Sum(), 2);
                allTrans.Add(new TransactionsTable { TransactionDate = "Report Total", TransactionAmount = transTotal });
                AllTransActions = allTrans.Select(x => x);
            }

            else if (type == "Revenue & Expense")
            {
                var revExpTrans = trans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Expense").ToList();
                var types = revExpTrans.GroupBy(x => x.TransactionType).Select(x => x.Key).Select(x => new
                {
                    Type = x,
                    Count = revExpTrans.Where(y => y.TransactionType == x).Count(),
                    Total = Math.Round(revExpTrans.Where(y => y.TransactionType == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() - revExpTrans.Where(y => y.TransactionType == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
                }).ToList();
                var transDates = revExpTrans.Select(x => x.TransactionDate).ToList();
                int count = 0;
                for (int i = 0; i < types.Count(); i++)
                {
                    transDates.Insert(count + types[i].Count, $"({i})");
                    count += types[i].Count + 1;
                    revExpTrans.Insert(transDates.IndexOf($"({i})"), new TransactionsTable { TransactionAmount = types[i].Total });
                }

                var revExpTotal = Math.Round(revExpTrans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Adjustment Increase").Select(x => x.TransactionAmount).Sum() - revExpTrans.Where(x => x.TransactionType == "Expense" || x.TransactionType == "Adjustment Decrease").Select(x => x.TransactionAmount).Sum(), 2);
                revExpTrans.Add(new TransactionsTable { TransactionDate = "Report Total", TransactionAmount = revExpTotal });
                AllTransActions = revExpTrans.Select(x => x);
            }

            else if (type == "Adjustment Increase & Adjustment Decrease")
            {
                var adjustTrans = trans.Where(x => x.TransactionType == "Adjustment Increase" || x.TransactionType == "Adjustment Decrease").ToList();
                var types = adjustTrans.GroupBy(x => x.TransactionType).Select(x => x.Key).Select(x => new
                {
                    Type = x,
                    Count = adjustTrans.Where(y => y.TransactionType == x).Count(),
                    Total = Math.Round(adjustTrans.Where(y => y.TransactionType == x).Where(y => y.TransactionType == "Revenue" || y.TransactionType == "Adjustment Increase").Select(y => y.TransactionAmount).Sum() - adjustTrans.Where(y => y.TransactionType == x).Where(y => y.TransactionType == "Expense" || y.TransactionType == "Adjustment Decrease").Select(y => y.TransactionAmount).Sum(), 2)
                }).ToList();
                var transDates = adjustTrans.Select(x => x.TransactionDate).ToList();
                int count = 0;
                for (int i = 0; i < types.Count(); i++)
                {
                    transDates.Insert(count + types[i].Count, $"({i})");
                    count += types[i].Count + 1;
                    adjustTrans.Insert(transDates.IndexOf($"({i})"), new TransactionsTable { TransactionAmount = types[i].Total });
                }

                var adjustTotal = Math.Round(adjustTrans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Adjustment Increase").Select(x => x.TransactionAmount).Sum() - adjustTrans.Where(x => x.TransactionType == "Expense" || x.TransactionType == "Adjustment Decrease").Select(x => x.TransactionAmount).Sum(), 2);
                adjustTrans.Add(new TransactionsTable { TransactionDate = "Report Total", TransactionAmount = adjustTotal });
                AllTransActions = adjustTrans.Select(x => x);
            }

            else
            {
                var specificTrans = trans.Where(x => x.TransactionType == type).ToList();
                var specificTotal = Math.Round(specificTrans.Where(x => x.TransactionType == "Revenue" || x.TransactionType == "Adjustment Increase").Select(x => x.TransactionAmount).Sum() - specificTrans.Where(x => x.TransactionType == "Expense" || x.TransactionType == "Adjustment Decrease").Select(x => x.TransactionAmount).Sum(), 2);
                specificTrans.Add(new TransactionsTable { TransactionDate = "Report Total", TransactionAmount = specificTotal });
                AllTransActions = specificTrans.Select(x => x);
            }
        }
    }
}
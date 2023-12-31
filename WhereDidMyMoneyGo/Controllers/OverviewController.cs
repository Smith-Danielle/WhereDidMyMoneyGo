﻿using System;
using Microsoft.AspNetCore.Mvc;
using WhereDidMyMoneyGo.Models;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WhereDidMyMoneyGo.Controllers
{
    //This controller users viewmodel (combining UsersModel and TransactionsModel)
    public class OverviewController : Controller
    {
        public OverviewController()
        {
        }

        //Anytime the Overview Page is accessed, this method should be called, needs usermodel object fed through parameter each time (from login page)
        public IActionResult OverviewIndex(UsersModel user)
        {
            var userInfo = user.GetUser(user.UserName);
            user.UserId = userInfo.First().UserId;
            user.FirstName = userInfo.First().FirstName;
            user.Balance = userInfo.First().Balance.ToString("0.00");

            TransactionsModel trans = new TransactionsModel();
            trans.OverviewInfo(user.UserId);
            ViewBag.DataPointsTransType = JsonConvert.SerializeObject(trans.DataPointsType);
            ViewBag.DataPointsMonthDay = JsonConvert.SerializeObject(trans.DataPointsMonthly);
            ViewBag.DataPointsYearMonth = JsonConvert.SerializeObject(trans.DataPointsYearly);
            ViewBag.DataPointsVendorActivity = JsonConvert.SerializeObject(trans.DataPointsVendor);
            ViewBag.DataPointsCategoryActivity = JsonConvert.SerializeObject(trans.DataPointsCategory);

            OverviewViewModel over = new OverviewViewModel();
            over.OverUsersModel = user;
            over.OverTransactionsModel = trans;

            return View(over);
        }

        //This does the same as the action above but is done when navbar Home Button is selected. Username is passed as object from navbar.
        public IActionResult OverviewIndexNav(string userName)
        {
            UsersModel user = new UsersModel();
            var userInfo = user.GetUser(userName);
            user.UserId = userInfo.First().UserId;
            user.UserName = userInfo.First().UserName;
            user.FirstName = userInfo.First().FirstName;
            user.Balance = userInfo.First().Balance.ToString("0.00");

            TransactionsModel trans = new TransactionsModel();
            trans.OverviewInfo(user.UserId);
            ViewBag.DataPointsTransType = JsonConvert.SerializeObject(trans.DataPointsType);
            ViewBag.DataPointsMonthDay = JsonConvert.SerializeObject(trans.DataPointsMonthly);
            ViewBag.DataPointsYearMonth = JsonConvert.SerializeObject(trans.DataPointsYearly);
            ViewBag.DataPointsVendorActivity = JsonConvert.SerializeObject(trans.DataPointsVendor);
            ViewBag.DataPointsCategoryActivity = JsonConvert.SerializeObject(trans.DataPointsCategory);

            OverviewViewModel over = new OverviewViewModel();
            over.OverUsersModel = user;
            over.OverTransactionsModel = trans;

            return View("OverviewIndex", over);
        }

        //Tab on Overview Page: Activity Entry
        public ActionResult ActivityEntry(string userName)
        {
            UsersModel user = new UsersModel();
            var userInfo = user.GetUser(userName);
            user.UserId = userInfo.First().UserId;
            user.UserName = userInfo.First().UserName;
            user.Balance = userInfo.First().Balance.ToString("0.00");

            OverviewViewModel over = new OverviewViewModel();
            over.OverUsersModel = user;

            return View(over);
        }

        //Enter Transaction - Activity Entry Page
        public ActionResult EnterTrans(string userName, List<string> messages = null, List<string> completedReq = null)
        {
            UsersModel user = new UsersModel();
            var userInfo = user.GetUser(userName);
            user.UserId = userInfo.First().UserId;
            user.UserName = userInfo.First().UserName;
            user.Balance = userInfo.First().Balance.ToString("0.00");

            TransactionsModel trans = new TransactionsModel();

            VendorsModel vendor = new VendorsModel();
            vendor.GetAllVendorsSelect(user.UserId);
            vendor.AllVendorsSelect = vendor.AllVendorsSelect.Where(x => x.Text != "ALL VENDORS");

            CategoriesModel cat = new CategoriesModel();
            cat.GetAllCategoriesSelect(user.UserId);
            cat.AllCategoriesSelect = cat.AllCategoriesSelect.Where(x => x.Text != "ALL CATEGORIES");

            OverviewViewModel over = new OverviewViewModel();
            over.OverUsersModel = user;
            over.OverTransactionsModel = trans;
            over.OverVendorsModel = vendor;
            over.OverCategoriesModel = cat;
            if (messages != null)
            {
                over.Messages = messages;
            }
            else
            {
                over.Messages = new List<string>();
            }
            if (completedReq != null)
            {
                over.CompletedRequest = completedReq;
            }
            else
            {
                over.CompletedRequest = new List<string>();
            }

            return View(over); 
        }
        
        //Submit on Enter Transaction Page
        public ActionResult FormEnterTrans(OverviewViewModel overview)
        {
            bool addVendor = false;
            bool addCategory = false;
            overview.Messages = new List<string>();
            overview.CompletedRequest = new List<string>();
            overview.OverVendorsModel.GetAllVendorsSelect(overview.OverUsersModel.UserId);
            overview.OverVendorsModel.AllVendorsSelect = overview.OverVendorsModel.AllVendorsSelect.Where(x => x.Text != "ALL VENDORS");
            overview.OverCategoriesModel.GetAllCategoriesSelect(overview.OverUsersModel.UserId);
            overview.OverCategoriesModel.AllCategoriesSelect = overview.OverCategoriesModel.AllCategoriesSelect.Where(x => x.Text != "ALL CATEGORIES");

            if (string.IsNullOrEmpty(overview.OverVendorsModel.VendorName) || string.IsNullOrEmpty(overview.OverCategoriesModel.CategoryName) || string.IsNullOrEmpty(overview.OverTransactionsModel.TransactionType) || string.IsNullOrEmpty(overview.OverTransactionsModel.TransactionAmount) || overview.OverTransactionsModel.TransactionDate.ToString("yyyy-MM-dd") == "0001-01-01")
            {
                overview.Messages.Add("All fields must be filled in to Submit.");
                return View("EnterTrans", overview);
            }

            var trimBalance = overview.OverTransactionsModel.TransactionAmount.Where(x => char.IsNumber(x) || x == '.');
            int decimalPlaces = overview.OverTransactionsModel.TransactionAmount.Contains('.') ? overview.OverTransactionsModel.TransactionAmount.Substring(overview.OverTransactionsModel.TransactionAmount.IndexOf('.') + 1).Length : 0;
            if (trimBalance.Count() != overview.OverTransactionsModel.TransactionAmount.Length || trimBalance.Where(x => x == '.').Count() > 1 || decimalPlaces > 2 || Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount) == 0)
            {
                overview.Messages.Add("Amount must be numerical, non-negative, larger than zero, no more than 2 decimal places.");
                return View("EnterTrans", overview);
            }

            if (overview.OverVendorsModel.DropDownVenOption == "*ADD NEW VENDOR*")
            {
                if (overview.OverVendorsModel.AllVendorsSelect.Select(x => x.Text.ToLower()).Contains(overview.OverVendorsModel.VendorName.ToLower()))
                {
                    overview.Messages.Add("Vendor already exists. Please select vendor from dropdown list or add new vendor.");
                    return View("EnterTrans", overview);
                }
                addVendor = true;
            }

            if (overview.OverCategoriesModel.DropDownCatOption == "*ADD NEW CATEGORY*")
            {
                if (overview.OverCategoriesModel.AllCategoriesSelect.Select(x => x.Text.ToLower()).Contains(overview.OverCategoriesModel.CategoryName.ToLower()))
                {
                    overview.Messages.Add("Category already exists. Please select category from dropdown list or add new category.");
                    return View("EnterTrans", overview);
                }
                addCategory = true;
            }

            //Proceed with database tranasctions after checks above
            //Add Vendor
            if (addVendor)
            {
                overview.OverVendorsModel.AddNewVendor(overview.OverUsersModel.UserId, overview.OverVendorsModel.VendorName);
                overview.Messages.Add($"Vendor: {overview.OverVendorsModel.VendorName} has been added.");
            }
            //Add Category
            if (addCategory)
            {
                overview.OverCategoriesModel.AddNewCategory(overview.OverUsersModel.UserId, overview.OverCategoriesModel.CategoryName);
                overview.Messages.Add($"Category: {overview.OverCategoriesModel.CategoryName} has been added.");
            }
            //Add Transaction
            overview.OverVendorsModel.GetVendors(overview.OverUsersModel.UserId);
            int venId = overview.OverVendorsModel.AllVendorsInfo.Where(x => x.VendorName.ToLower() == overview.OverVendorsModel.VendorName.ToLower()).Select(x => x.VendorId).First();

            overview.OverCategoriesModel.GetCategories(overview.OverUsersModel.UserId);
            int catId = overview.OverCategoriesModel.AllCategoriesInfo.Where(x => x.CategoryName.ToLower() == overview.OverCategoriesModel.CategoryName.ToLower()).Select(x => x.CategoryId).First();

            overview.OverTransactionsModel.AddNewTrans(overview.OverUsersModel.UserId, venId, catId, overview.OverTransactionsModel.TransactionDate.ToString("yyyy-MM-dd"), overview.OverTransactionsModel.TransactionType, Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount));
            overview.Messages.Add("The following Transaction has been recorded:");
            overview.CompletedRequest.Add(overview.OverTransactionsModel.TransactionDate.ToString("MM-dd-yyyy"));
            overview.CompletedRequest.Add(overview.OverVendorsModel.VendorName);
            overview.CompletedRequest.Add(overview.OverCategoriesModel.CategoryName);
            overview.CompletedRequest.Add(overview.OverTransactionsModel.TransactionType);
            overview.CompletedRequest.Add(Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount).ToString("0.00"));
            //Update User Balance
            overview.OverUsersModel.UpdateBalance(overview.OverUsersModel.UserId, Convert.ToDouble(overview.OverUsersModel.Balance), Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount), overview.OverTransactionsModel.TransactionType);

            return RedirectToAction("EnterTrans", "Overview", new {userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest });

        }

        //Adjust Balance - Activity Entry Page
        public ActionResult AdjustBalance(string userName, List<string> messages = null, List<string> completedReq = null)
        {
            UsersModel user = new UsersModel();
            var userInfo = user.GetUser(userName);
            user.UserId = userInfo.First().UserId;
            user.UserName = userInfo.First().UserName;
            user.Balance = userInfo.First().Balance.ToString("0.00");

            TransactionsModel trans = new TransactionsModel();

            OverviewViewModel over = new OverviewViewModel();
            over.OverUsersModel = user;
            over.OverTransactionsModel = trans;
            if (messages != null)
            {
                over.Messages = messages;
            }
            else
            {
                over.Messages = new List<string>();
            }
            if (completedReq != null)
            {
                over.CompletedRequest = completedReq;
            }
            else
            {
                over.CompletedRequest = new List<string>();
            }

            return View(over);
        }

        //Submit on Adjust Balance Page
        public ActionResult FormAdjustBalance(OverviewViewModel overview)
        {
            overview.Messages = new List<string>();
            overview.CompletedRequest = new List<string>();

            if (string.IsNullOrEmpty(overview.OverTransactionsModel.TransactionType) || string.IsNullOrEmpty(overview.OverTransactionsModel.TransactionAmount) || overview.OverTransactionsModel.TransactionDate.ToString("yyyy-MM-dd") == "0001-01-01")
            {
                overview.Messages.Add("All fields must be filled in to Submit.");
                return View("AdjustBalance", overview);
            }

            var trimBalance = overview.OverTransactionsModel.TransactionAmount.Where(x => char.IsNumber(x) || x == '.');
            int decimalPlaces = overview.OverTransactionsModel.TransactionAmount.Contains('.') ? overview.OverTransactionsModel.TransactionAmount.Substring(overview.OverTransactionsModel.TransactionAmount.IndexOf('.') + 1).Length : 0;
            if (trimBalance.Count() != overview.OverTransactionsModel.TransactionAmount.Length || trimBalance.Where(x => x == '.').Count() > 1 || decimalPlaces > 2 || Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount) == 0)
            {
                overview.Messages.Add("Amount must be numerical, non-negative, larger than zero, no more than 2 decimal places.");
                return View("AdjustBalance", overview);
            }

            //Proceed with database tranasctions after checks above
            //Add Transaction (adjustment)
            VendorsModel ven = new VendorsModel();
            ven.GetVendors(overview.OverUsersModel.UserId);
            int venId = ven.AllVendorsInfo.Where(x => x.VendorName == "User Adjustment").Select(x => x.VendorId).First();

            CategoriesModel cat = new CategoriesModel();
            cat.GetCategories(overview.OverUsersModel.UserId);
            int catId = cat.AllCategoriesInfo.Where(x => x.CategoryName == "Balance Adjustment").Select(x => x.CategoryId).First();

            overview.OverTransactionsModel.AddNewTrans(overview.OverUsersModel.UserId, venId, catId, overview.OverTransactionsModel.TransactionDate.ToString("yyyy-MM-dd"), overview.OverTransactionsModel.TransactionType, Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount));
            overview.Messages.Add("The following Adjustment has been recorded:");
            overview.CompletedRequest.Add(overview.OverTransactionsModel.TransactionDate.ToString("MM-dd-yyyy"));
            overview.CompletedRequest.Add(overview.OverTransactionsModel.TransactionType);
            overview.CompletedRequest.Add(Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount).ToString("0.00"));
            //Update User Balance
            overview.OverUsersModel.UpdateBalance(overview.OverUsersModel.UserId, Convert.ToDouble(overview.OverUsersModel.Balance), Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount), overview.OverTransactionsModel.TransactionType);

            return RedirectToAction("AdjustBalance", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest });


        }

        //Tab on Overview Page: Vendors & Categories
        public ActionResult VendorCategory(string userName, List<string> messages = null, List<string> completedReq = null)
        {
            UsersModel user = new UsersModel();
            var userInfo = user.GetUser(userName);
            user.UserId = userInfo.First().UserId;
            user.UserName = userInfo.First().UserName;
            user.Balance = userInfo.First().Balance.ToString("0.00");

            VendorsModel vendor = new VendorsModel();
            vendor.GetAllVendorsSelect(user.UserId);
            vendor.AllVendorsSelect = vendor.AllVendorsSelect.Where(x => x.Text != "*ADD NEW VENDOR*" && x.Text != "ALL VENDORS");
            vendor.GetVendors(user.UserId);
            vendor.AllVendorsInfo = vendor.AllVendorsInfo.Where(x => x.VendorName != "User Adjustment");

            CategoriesModel cat = new CategoriesModel();
            cat.GetAllCategoriesSelect(user.UserId);
            cat.AllCategoriesSelect = cat.AllCategoriesSelect.Where(x => x.Text != "*ADD NEW CATEGORY*" && x.Text != "ALL CATEGORIES");
            cat.GetCategories(user.UserId);
            cat.AllCategoriesInfo = cat.AllCategoriesInfo.Where(x => x.CategoryName != "Balance Adjustment");

            OverviewViewModel over = new OverviewViewModel();
            over.OverUsersModel = user;
            over.OverVendorsModel = vendor;
            over.OverCategoriesModel = cat;
            if (messages != null)
            {
                over.Messages = messages;
            }
            else
            {
                over.Messages = new List<string>();
            }
            if (completedReq != null)
            {
                over.CompletedRequest = completedReq;
            }
            else
            {
                over.CompletedRequest = new List<string>();
            }

            return View(over);
        }

        //Add or Delete on Vendors & Categories Page
        public ActionResult FormVendorModify(string command, OverviewViewModel overview)
        {
            overview.Messages = new List<string>();
            overview.CompletedRequest = new List<string>();

            if (command == "Add")
            {
                if (string.IsNullOrEmpty(overview.OverVendorsModel.VendorName))
                {
                    overview.Messages.Add("Vendor");
                    overview.Messages.Add("Please fill in the corresponding field to Add a new Vendor.");
                    return RedirectToAction("VendorCategory", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest });
                }

                overview.OverVendorsModel.GetVendors(overview.OverUsersModel.UserId);
                if (overview.OverVendorsModel.AllVendorsInfo.Select(x => x.VendorName.ToLower()).Contains(overview.OverVendorsModel.VendorName.ToLower()))
                {
                    overview.Messages.Add("Vendor");
                    overview.Messages.Add($"Vendor: {overview.OverVendorsModel.VendorName} already exists.");
                }
                else
                {
                    overview.OverVendorsModel.AddNewVendor(overview.OverUsersModel.UserId, overview.OverVendorsModel.VendorName);
                    overview.CompletedRequest.Add("Vendor");
                    overview.CompletedRequest.Add($"Vendor: {overview.OverVendorsModel.VendorName} has been added.");
                }
                return RedirectToAction("VendorCategory", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest });
            }
            else
            {
                if (string.IsNullOrEmpty(overview.OverVendorsModel.DropDownVenOption))
                {
                    overview.Messages.Add("Vendor");
                    overview.Messages.Add("Please select a Vendor from the corresponding field to Delete.");
                    return RedirectToAction("VendorCategory", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest });
                }

                TransactionsModel trans = new TransactionsModel();
                trans.GetAllUserTrans(overview.OverUsersModel.UserId);
                if (trans.AllTransActions.Select(x => x.VendorName.ToLower()).Contains(overview.OverVendorsModel.DropDownVenOption.ToLower()))
                {
                    overview.Messages.Add("Vendor");
                    overview.Messages.Add($"Vendor: {overview.OverVendorsModel.DropDownVenOption} is tied to a pre-existing transaction and cannot be deleted.");
                }
                else
                {
                    overview.OverVendorsModel.DeleteVendor(overview.OverUsersModel.UserId, overview.OverVendorsModel.DropDownVenOption);
                    overview.CompletedRequest.Add("Vendor");
                    overview.CompletedRequest.Add($"Vendor: {overview.OverVendorsModel.DropDownVenOption} has been deleted.");
                }
                return RedirectToAction("VendorCategory", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest});
            }
        }


        //Add or Delete on Vendors & Categories Page
        public ActionResult FormCategoryModify(string command, OverviewViewModel overview)
        {
            overview.Messages = new List<string>();
            overview.CompletedRequest = new List<string>();

            if (command == "Add")
            {
                if (string.IsNullOrEmpty(overview.OverCategoriesModel.CategoryName))
                {
                    overview.Messages.Add("Category");
                    overview.Messages.Add("Please fill in the corresponding field to Add a new Category.");
                    return RedirectToAction("VendorCategory", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest });
                }

                overview.OverCategoriesModel.GetCategories(overview.OverUsersModel.UserId);
                if (overview.OverCategoriesModel.AllCategoriesInfo.Select(x => x.CategoryName.ToLower()).Contains(overview.OverCategoriesModel.CategoryName.ToLower()))
                {
                    overview.Messages.Add("Category");
                    overview.Messages.Add($"Category: {overview.OverCategoriesModel.CategoryName} already exists.");
                }
                else
                {
                    overview.OverCategoriesModel.AddNewCategory(overview.OverUsersModel.UserId, overview.OverCategoriesModel.CategoryName);
                    overview.CompletedRequest.Add("Category");
                    overview.CompletedRequest.Add($"Category: {overview.OverCategoriesModel.CategoryName} has been added.");
                }
                return RedirectToAction("VendorCategory", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest });
            }
            else
            {
                if (string.IsNullOrEmpty(overview.OverCategoriesModel.DropDownCatOption))
                {
                    overview.Messages.Add("Category");
                    overview.Messages.Add("Please select a Category from the corresponding field to Delete.");
                    return RedirectToAction("VendorCategory", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest });
                }

                TransactionsModel trans = new TransactionsModel();
                trans.GetAllUserTrans(overview.OverUsersModel.UserId);
                if (trans.AllTransActions.Select(x => x.CategoryName.ToLower()).Contains(overview.OverCategoriesModel.DropDownCatOption.ToLower()))
                {
                    overview.Messages.Add("Category");
                    overview.Messages.Add($"Category: {overview.OverCategoriesModel.DropDownCatOption} is tied to a pre-existing transaction and cannot be deleted.");
                }
                else
                {
                    overview.OverCategoriesModel.DeleteCategory(overview.OverUsersModel.UserId, overview.OverCategoriesModel.DropDownCatOption);
                    overview.CompletedRequest.Add("Category");
                    overview.CompletedRequest.Add($"Category: {overview.OverCategoriesModel.DropDownCatOption} has been deleted.");
                }
                return RedirectToAction("VendorCategory", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedReq = overview.CompletedRequest });
            }
        }

        //Tab on Overview Page: Reports
        public ActionResult Reports(string userName)
        {
            UsersModel user = new UsersModel();
            var userInfo = user.GetUser(userName);
            user.UserId = userInfo.First().UserId;
            user.UserName = userInfo.First().UserName;
            user.Balance = userInfo.First().Balance.ToString("0.00");

            TransactionsModel trans = new TransactionsModel();

            VendorsModel vendor = new VendorsModel();
            vendor.GetAllVendorsSelect(user.UserId);
            vendor.AllVendorsSelect = vendor.AllVendorsSelect.Where(x => x.Text != "*ADD NEW VENDOR*");

            CategoriesModel cat = new CategoriesModel();
            cat.GetAllCategoriesSelect(user.UserId);
            cat.AllCategoriesSelect = cat.AllCategoriesSelect.Where(x => x.Text != "*ADD NEW CATEGORY*");

            OverviewViewModel over = new OverviewViewModel();
            over.OverUsersModel = user;
            over.OverTransactionsModel = trans;
            over.OverVendorsModel = vendor;
            over.OverCategoriesModel = cat;
            over.Messages = new List<string>();
            over.CompletedRequest = new List<string>();

            return View(over);
        }

        //Report Options on Report Page
        public ActionResult FormReports(string command, OverviewViewModel overview)
        {
            overview.Messages = new List<string>();
            overview.CompletedRequest = new List<string>();
            overview.OverVendorsModel.GetAllVendorsSelect(overview.OverUsersModel.UserId);
            overview.OverVendorsModel.AllVendorsSelect = overview.OverVendorsModel.AllVendorsSelect.Where(x => x.Text != "*ADD NEW VENDOR*");
            overview.OverCategoriesModel.GetAllCategoriesSelect(overview.OverUsersModel.UserId);
            overview.OverCategoriesModel.AllCategoriesSelect = overview.OverCategoriesModel.AllCategoriesSelect.Where(x => x.Text != "*ADD NEW CATEGORY*");

            Dictionary<string, List<string>> reportType = new Dictionary<string, List<string>>
            {
                { "Entry Report", new List<string> { "Group", overview.OverTransactionsModel.DropDownGroupSelection, "Activity Entry" } },
                { "Vendor Report", new List<string> { "Vendor", overview.OverVendorsModel.DropDownVenOption, "Vendor" } },
                { "Category Report", new List<string> { "Category", overview.OverCategoriesModel.DropDownCatOption, "Category" } },
                { "Type Report", new List<string> { "Type", overview.OverTransactionsModel.DropDownTypeSelection, "Activity Type" } }
            };

            if (string.IsNullOrEmpty(reportType[command][1]))
            {
                overview.Messages.Add($"{command}: A {reportType[command][0]} option must be selected to run report.");
                return View("Reports", overview);
            }

            if (command == "Entry Report")
            {
                if (overview.OverTransactionsModel.EndDateEntry.ToString("yyyy-MM-dd") == "0001-01-01")
                {
                    overview.OverTransactionsModel.EndDateEntry = DateTime.Now;
                }
                if (overview.OverTransactionsModel.StartDateEntry > overview.OverTransactionsModel.EndDateEntry)
                {
                    overview.Messages.Add($"{command}: Start Date must be before End Date.");
                    return View("Reports", overview);
                }
                overview.OverTransactionsModel.EntryReport(overview.OverUsersModel.UserId, reportType[command][1], overview.OverTransactionsModel.StartDateEntry, overview.OverTransactionsModel.EndDateEntry);
            }

            if (command == "Vendor Report")
            {
                if (overview.OverTransactionsModel.EndDateVendor.ToString("yyyy-MM-dd") == "0001-01-01")
                {
                    overview.OverTransactionsModel.EndDateVendor = DateTime.Now;
                }
                if (overview.OverTransactionsModel.StartDateVendor > overview.OverTransactionsModel.EndDateVendor)
                {
                    overview.Messages.Add($"{command}: Start Date must be before End Date.");
                    return View("Reports", overview);
                }
                overview.OverTransactionsModel.VendorReport(overview.OverUsersModel.UserId, reportType[command][1], overview.OverTransactionsModel.StartDateVendor, overview.OverTransactionsModel.EndDateVendor);
            }

            if (command == "Category Report")
            {
                if (overview.OverTransactionsModel.EndDateCategory.ToString("yyyy-MM-dd") == "0001-01-01")
                {
                    overview.OverTransactionsModel.EndDateCategory = DateTime.Now;
                }
                if (overview.OverTransactionsModel.StartDateCategory > overview.OverTransactionsModel.EndDateCategory)
                {
                    overview.Messages.Add($"{command}: Start Date must be before End Date.");
                    return View("Reports", overview);
                }
                overview.OverTransactionsModel.CategoryReport(overview.OverUsersModel.UserId, reportType[command][1], overview.OverTransactionsModel.StartDateCategory, overview.OverTransactionsModel.EndDateCategory);
            }

            if (command == "Type Report")
            {
                if (overview.OverTransactionsModel.EndDateType.ToString("yyyy-MM-dd") == "0001-01-01")
                {
                    overview.OverTransactionsModel.EndDateType = DateTime.Now;
                }
                if (overview.OverTransactionsModel.StartDateType > overview.OverTransactionsModel.EndDateType)
                {
                    overview.Messages.Add($"{command}: Start Date must be before End Date.");
                    return View("Reports", overview);
                }
                overview.OverTransactionsModel.TypeReport(overview.OverUsersModel.UserId, reportType[command][1], overview.OverTransactionsModel.StartDateType, overview.OverTransactionsModel.EndDateType);
            }

            var tempAllTrans = overview.OverTransactionsModel.AllTransActions.ToList();
            var tempUser = overview.OverUsersModel.UserName;
            var tempId = overview.OverUsersModel.UserId;
            var tempBal = overview.OverUsersModel.Balance;
            var tempStart = command == "Entry Report" ? overview.OverTransactionsModel.StartDateEntry : command == "Vendor Report" ? overview.OverTransactionsModel.StartDateVendor : command == "Category Report" ? overview.OverTransactionsModel.StartDateCategory : overview.OverTransactionsModel.StartDateType;
            var tempEnd = command == "Entry Report" ? overview.OverTransactionsModel.EndDateEntry : command == "Vendor Report" ? overview.OverTransactionsModel.EndDateVendor : command == "Category Report" ? overview.OverTransactionsModel.EndDateCategory : overview.OverTransactionsModel.EndDateType;

            ModelState.Clear();

            overview.OverTransactionsModel = new TransactionsModel();
            overview.OverUsersModel = new UsersModel();
            overview.OverVendorsModel = new VendorsModel();
            overview.OverCategoriesModel = new CategoriesModel();
            overview.CompletedRequest = new List<string>();

            overview.OverTransactionsModel.AllTransActions = tempAllTrans;
            overview.OverUsersModel.UserName = tempUser;
            overview.OverUsersModel.UserId = tempId;
            overview.OverUsersModel.Balance = tempBal;
            overview.OverVendorsModel.GetAllVendorsSelect(overview.OverUsersModel.UserId);
            overview.OverVendorsModel.AllVendorsSelect = overview.OverVendorsModel.AllVendorsSelect.Where(x => x.Text != "*ADD NEW VENDOR*");
            overview.OverCategoriesModel.GetAllCategoriesSelect(overview.OverUsersModel.UserId);
            overview.OverCategoriesModel.AllCategoriesSelect = overview.OverCategoriesModel.AllCategoriesSelect.Where(x => x.Text != "*ADD NEW CATEGORY*");
            overview.CompletedRequest.Add($"{reportType[command][2]} Report");
            overview.CompletedRequest.Add($"{reportType[command][0]}: {reportType[command][1]}");
            overview.CompletedRequest.Add($"Dates: Between {tempStart.ToString("MM-dd-yyyy")} and {tempEnd.ToString("MM-dd-yyyy")}");

            return View("Reports", overview);
        }

        //Tab on Overview Page: Profile
        public ActionResult Profile(string userName)
        {
            UsersModel user = new UsersModel();
            var userInfo = user.GetUser(userName);
            user.UserId = userInfo.First().UserId;
            user.UserName = userInfo.First().UserName;
            user.Balance = userInfo.First().Balance.ToString("0.00");
            user.FirstName = userInfo.First().FirstName;
            user.LastName = userInfo.First().LastName;
            user.Password = userInfo.First().Password;
            user.SecurityAnswer = userInfo.First().SecurityAnswer;

            OverviewViewModel over = new OverviewViewModel();
            over.OverUsersModel = user;

            return View(over);
        }

        //Change Password - Profile Page
        public ActionResult ChangePassword(OverviewViewModel overview)
        {
            return View(overview);
        }

        //Submit Password on Change Password Page
        public ActionResult FormChangePassword(OverviewViewModel overview)
        {
            var userInfo = overview.OverUsersModel.GetUser(overview.OverUsersModel.UserName);

            overview.OverUsersModel.UpdatePassword(overview.OverUsersModel.UserId, overview.OverUsersModel.Password, userInfo.First().Password);
            if (overview.OverUsersModel.Message == "Your password has successfully been updated.")
            {
                //to clear password field on form on success
                var tempUser = overview.OverUsersModel.UserName;
                var tempId = overview.OverUsersModel.UserId;
                var tempBal = overview.OverUsersModel.Balance;
                var tempMess = overview.OverUsersModel.Message;

                ModelState.Clear();

                overview.OverUsersModel = new UsersModel();

                overview.OverUsersModel.UserName = tempUser;
                overview.OverUsersModel.UserId = tempId;
                overview.OverUsersModel.Balance = tempBal;
                overview.OverUsersModel.Message = tempMess;
            }
            return View("ChangePassword", overview);
        }

        //Change Security Answer - Profile Page
        public ActionResult ChangeSecurityAnswer(OverviewViewModel overview)
        {
            return View(overview);
        }

        //Submit Password on Change Security Answer Page
        public ActionResult FormChangeSecureAns(OverviewViewModel overview)
        {
            var userInfo = overview.OverUsersModel.GetUser(overview.OverUsersModel.UserName);

            overview.OverUsersModel.UpdateSecurityAnswer(overview.OverUsersModel.UserId, overview.OverUsersModel.SecurityAnswer, userInfo.First().SecurityAnswer);
            if (overview.OverUsersModel.Message == "Your security answer has successfully been updated.")
            {
                //to clear password field on form on success
                var tempUser = overview.OverUsersModel.UserName;
                var tempId = overview.OverUsersModel.UserId;
                var tempBal = overview.OverUsersModel.Balance;
                var tempMess = overview.OverUsersModel.Message;

                ModelState.Clear();

                overview.OverUsersModel = new UsersModel();

                overview.OverUsersModel.UserName = tempUser;
                overview.OverUsersModel.UserId = tempId;
                overview.OverUsersModel.Balance = tempBal;
                overview.OverUsersModel.Message = tempMess;
            }
            return View("ChangeSecurityAnswer", overview);
        }

        //Tab on Overview Page: Logout, back to Login Page
        public ActionResult Logout()
        {
            return RedirectToAction("EntryIndex", "Entry");
        }
    }
}

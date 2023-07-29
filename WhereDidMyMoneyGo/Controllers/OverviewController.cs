using System;
using Microsoft.AspNetCore.Mvc;
using WhereDidMyMoneyGo.Models;
using System.Linq;
using System.Collections.Generic;

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
            trans.TopTrans(user.UserId);

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
            trans.TopTrans(user.UserId);

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
        public ActionResult EnterTrans(string userName, List<string> messages = null, List<string> completedTrans = null)
        {
            UsersModel user = new UsersModel();
            var userInfo = user.GetUser(userName);
            user.UserId = userInfo.First().UserId;
            user.UserName = userInfo.First().UserName;
            user.Balance = userInfo.First().Balance.ToString("0.00");

            TransactionsModel trans = new TransactionsModel();

            VendorsModel vendor = new VendorsModel();
            vendor.GetAllVendorsSelect(user.UserId);

            CategoriesModel cat = new CategoriesModel();
            cat.GetAllCategoriesSelect(user.UserId);

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
            if (completedTrans != null)
            {
                over.CompletedTransaction = completedTrans;
            }
            else
            {
                over.CompletedTransaction = new List<string>();
            }

            return View(over); 
        }
        
        //Submit on Enter Transaction Page
        public ActionResult FormEnterTrans(OverviewViewModel overview)
        {
            bool addVendor = false;
            bool addCategory = false;
            overview.Messages = new List<string>();
            overview.CompletedTransaction = new List<string>();
            overview.OverVendorsModel.GetAllVendorsSelect(overview.OverUsersModel.UserId);
            overview.OverCategoriesModel.GetAllCategoriesSelect(overview.OverUsersModel.UserId);

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
            overview.CompletedTransaction.Add(overview.OverTransactionsModel.TransactionDate.ToString("MM-dd-yyyy"));
            overview.CompletedTransaction.Add(overview.OverVendorsModel.VendorName);
            overview.CompletedTransaction.Add(overview.OverCategoriesModel.CategoryName);
            overview.CompletedTransaction.Add(overview.OverTransactionsModel.TransactionType);
            overview.CompletedTransaction.Add(Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount).ToString("0.00"));
            //Update User Balance
            overview.OverUsersModel.UpdateBalance(overview.OverUsersModel.UserId, Convert.ToDouble(overview.OverUsersModel.Balance), Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount), overview.OverTransactionsModel.TransactionType);

            return RedirectToAction("EnterTrans", "Overview", new {userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedTrans = overview.CompletedTransaction });

        }

        //Adjust Balance - Activity Entry Page
        public ActionResult AdjustBalance(string userName, List<string> messages = null, List<string> completedTrans = null)
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
            if (completedTrans != null)
            {
                over.CompletedTransaction = completedTrans;
            }
            else
            {
                over.CompletedTransaction = new List<string>();
            }

            return View(over);
        }

        //Submit on Adjust Balance Page
        public ActionResult FormAdjustBalance(OverviewViewModel overview)
        {
            overview.Messages = new List<string>();
            overview.CompletedTransaction = new List<string>();

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
            int venId = 5; //Hard coded from database for 'User Adjustment'

            int catId = 17; //Hard coded from database for 'Balance Adjustment'

            overview.OverTransactionsModel.AddNewTrans(overview.OverUsersModel.UserId, venId, catId, overview.OverTransactionsModel.TransactionDate.ToString("yyyy-MM-dd"), overview.OverTransactionsModel.TransactionType, Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount));
            overview.Messages.Add("The following Adjustment has been recorded:");
            overview.CompletedTransaction.Add(overview.OverTransactionsModel.TransactionDate.ToString("MM-dd-yyyy"));
            overview.CompletedTransaction.Add(overview.OverTransactionsModel.TransactionType);
            overview.CompletedTransaction.Add(Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount).ToString("0.00"));
            //Update User Balance
            overview.OverUsersModel.UpdateBalance(overview.OverUsersModel.UserId, Convert.ToDouble(overview.OverUsersModel.Balance), Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount), overview.OverTransactionsModel.TransactionType);

            return RedirectToAction("AdjustBalance", "Overview", new { userName = overview.OverUsersModel.UserName, messages = overview.Messages, completedTrans = overview.CompletedTransaction });


        }










        //Tab on Overview Page: Activity Entry, back to Login Page
        public ActionResult Logout()
        {
            return RedirectToAction("EntryIndex", "Entry");
        }
    }
}

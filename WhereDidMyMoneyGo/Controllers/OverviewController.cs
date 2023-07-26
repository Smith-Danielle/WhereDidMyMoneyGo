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
            user.Balance = userInfo.First().Balance.ToString();

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
            user.Balance = userInfo.First().Balance.ToString();

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
            user.Balance = userInfo.First().Balance.ToString();

            OverviewViewModel over = new OverviewViewModel();
            over.OverUsersModel = user;

            return View(over);
        }

        //Enter Transaction - Activity Entry Page
        public ActionResult EnterTrans(int userId, string username, string balance, List<string> messages = null)
        {
            UsersModel user = new UsersModel();
            user.UserId = userId;
            user.UserName = username;
            user.Balance = balance;

            TransactionsModel trans = new TransactionsModel();

            VendorsModel vendor = new VendorsModel();
            vendor.GetAllVendors(userId);

            CategoriesModel cat = new CategoriesModel();
            cat.GetAllCategories(userId);

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
                over.Messages = new List<string>() { "" };
            }

            return View(over); 
        }
        
        //Submit on Enter Transaction Page
        public ActionResult FormEnterTrans(OverviewViewModel overview)
        {
            bool addVendor = false;
            bool addCategory = false;
            overview.Messages = new List<string>();

            if (string.IsNullOrEmpty(overview.OverVendorsModel.VendorName) || string.IsNullOrEmpty(overview.OverCategoriesModel.CategoryName) || string.IsNullOrEmpty(overview.OverTransactionsModel.TransactionType) || string.IsNullOrEmpty(overview.OverTransactionsModel.TransactionAmount) || overview.OverTransactionsModel.TransactionDate.ToString("yyyy-MM-dd") == "0001-01-01")
            {
                overview.Messages.Add("All fields must be filled in to Submit.");
                return View("EnterTrans", overview);
            }

            var trimBalance = overview.OverTransactionsModel.TransactionAmount.Where(x => char.IsNumber(x) || x == '.');
            if (trimBalance.Count() != overview.OverTransactionsModel.TransactionAmount.Length || trimBalance.Where(x => x == '.').Count() > 1 || Convert.ToDouble(overview.OverTransactionsModel.TransactionAmount) == 0)
            {
                overview.Messages.Add("Amount must be a non-negative numerical value. Larger than zero.");
                return View("EnterTrans", overview);
            }

            if (overview.OverVendorsModel.DropDownVenOption.Where(x => x.Text == "*ADD NEW VENDOR*").Count() > 1)
            {
                if (overview.OverVendorsModel.AllVendors.Select(x => x.Text.ToLower()).Contains(overview.OverVendorsModel.VendorName.ToLower()))
                {
                    overview.Messages.Add("Vendor already exists. Please select vendor from dropdown list or add new vendor.");
                    return View("EnterTrans", overview);
                }
                addVendor = true;
            }

            if (overview.OverCategoriesModel.DropDownCatOption.Where(x => x.Text == "*ADD NEW CATEGORY*").Count() > 1)
            {
                if (overview.OverCategoriesModel.AllCategories.Select(x => x.Text.ToLower()).Contains(overview.OverCategoriesModel.CategoryName.ToLower()))
                {
                    overview.Messages.Add("Category already exists. Please select category from dropdown list or add new category.");
                    return View("EnterTrans", overview);
                }
                addCategory = true;
            }

            //Proceed with database tranasctions after checks above

            if (addVendor)
            {
                //call method to add vendor
                overview.Messages.Add($"Vendor: {overview.OverVendorsModel.VendorName} has been added.");
            }

            if (addCategory)
            {
                //call method to add category
                overview.Messages.Add($"Category: {overview.OverCategoriesModel.CategoryName} has been added.");
            }

            //call method to add transaction
            //add transaction successful message with details

            //call method to update user balance

            
            return RedirectToAction("EnterTrans", "Overview", new { userId = overview.OverUsersModel.UserId, username = overview.OverUsersModel.UserName, balance = overview.OverUsersModel.Balance, messages = overview.Messages });

        }
        
        
        //Tab on Overview Page: Activity Entry, back to Login Page
        public ActionResult Logout()
        {
            return RedirectToAction("EntryIndex", "Entry");
        }
    }
}

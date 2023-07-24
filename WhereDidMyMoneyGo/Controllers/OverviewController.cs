using System;
using Microsoft.AspNetCore.Mvc;
using WhereDidMyMoneyGo.Models;
using System.Linq;

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
        public ActionResult EnterTrans(int userId, string username, string balance)
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

            return View(over); 

        }









        public ActionResult Logout()
        {
            return RedirectToAction("EntryIndex", "Entry");
        }
    }
}

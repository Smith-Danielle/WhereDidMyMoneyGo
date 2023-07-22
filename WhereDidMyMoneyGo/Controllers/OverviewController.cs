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

        //Anytime the Overview Page is accessed, this method should be called, needs usermodel object fed through parameter each time
        public IActionResult OverviewIndex(UsersModel user)
        {
            var userInfo = user.GetUser(user.UserName);
            user.UserId = userInfo.First().UserId;
            user.FirstName = userInfo.First().FirstName;
            user.LastName = userInfo.First().LastName;
            user.Balance = userInfo.First().Balance.ToString();
            user.Password = userInfo.First().Password;
            user.SecurityAnswer = userInfo.First().SecurityAnswer;

            TransactionsModel trans = new TransactionsModel();
            trans.TopTrans(user.UserId);

            OverviewViewModel over = new OverviewViewModel(user, trans);

            return View(over);
        }
    }
}

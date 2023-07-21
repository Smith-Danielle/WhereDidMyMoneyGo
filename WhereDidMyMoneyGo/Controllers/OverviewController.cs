using System;
using Microsoft.AspNetCore.Mvc;
using WhereDidMyMoneyGo.Models;

namespace WhereDidMyMoneyGo.Controllers
{
    //This controller users viewmodel (combining UsersModel and TransactionsModel)
    public class OverviewController : Controller
    {
        public OverviewController()
        {
        }

        public IActionResult OverviewIndex(UsersModel user)
        {
            TransactionsModel trans = new TransactionsModel();
            trans.TopTrans();
            OverviewViewModel over = new OverviewViewModel(user, trans);
            return View(over);
        }
    }
}

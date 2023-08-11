using System;
using Microsoft.AspNetCore.Mvc;
using WhereDidMyMoneyGo.Models;
using System.Linq;

namespace WhereDidMyMoneyGo.Controllers
{
    public class EntryController : Controller
    {
        public EntryController()
        {
        }

        public IActionResult EntryIndex()
        {
            //user is instatiated to create a clean user model for the view. view needs this up fron because it references model before values are assigned
            UsersModel user = new UsersModel();
            return View(user);
        }

        //Login Button from Entry Page
        public ActionResult FormLogin(UsersModel formUser)
        {
            formUser.Login(formUser.UserName, formUser.Password);
            if (formUser.Message == "Welcome.")
            {
                return RedirectToAction("OverviewIndex", "Overview", formUser);
            }
            else
            {
                return View("EntryIndex", formUser);
            }
        }

        //Forgot Password Button from Entry Page
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //Submit Button from Forgot Password Page
        public ActionResult FormForgotPass(UsersModel forgotUser)
        {
            forgotUser.GetPassword(forgotUser.UserName, forgotUser.SecurityAnswer);

            //clear the form after retrieval
            var tempMess = forgotUser.Message;
            ModelState.Clear();
            forgotUser = new UsersModel();
            forgotUser.Message = tempMess;

            return View("ForgotPassword", forgotUser);
        }

        //Create Login Button from Entry Page
        public ActionResult CreateLogin()
        {
            UsersModel user = new UsersModel();
            return View(user);
        }

        //Submit Button from Create Login Page
        public ActionResult FormCreateLogin(UsersModel createUser)
        {
            createUser.CreateNewUser(createUser.UserName, createUser.Password, createUser.Balance, createUser.FirstName, createUser.LastName, createUser.SecurityAnswer);
            if (createUser.Message == "Login successfully created.")
            {
                //add default vendor and cats to database for new user, then send them back to login
                UsersModel user = new UsersModel();
                var userInfo = user.GetUser(createUser.UserName);

                VendorsModel ven = new VendorsModel();
                ven.AddDefaultVendors(userInfo.First().UserId);

                CategoriesModel cat = new CategoriesModel();
                cat.AddDefaultCats(userInfo.First().UserId);

                //then clear the form on a success
                var tempMess = createUser.Message;
                ModelState.Clear();
                createUser = new UsersModel();
                createUser.Message = tempMess;
            }
            return View("CreateLogin", createUser);
        }
    }
}

using System;
using Microsoft.AspNetCore.Mvc;
using WhereDidMyMoneyGo.Models;

namespace WhereDidMyMoneyGo.Controllers
{
    public class EntryController : Controller
    {
        public EntryController()
        {
            UsersModel userModel = new UsersModel();
            user = userModel;
        }
        public UsersModel user { get; set; }

        public IActionResult EntryIndex()
        {
            //user is instatiated to create a clean user model for the view. view needs this up fron because it references model before values are assigned
            return View(user);
        }

        //Login Button from Entry Page
        public ActionResult FormLogin(UsersModel formUser)
        {
            formUser.Login(formUser.UserName, formUser.Password);
            if (formUser.Message == "Welcome.")
            {
                //return View();
                //User Home Page View will go here
                return RedirectToAction("EntryIndex", "Entry");
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
            return View("ForgotPassword", forgotUser);
        }

        //Create Login Button from Entry Page
        public ActionResult CreateLogin()
        {
            return View();
        }

        //Submit Button from Create Login Page
        public ActionResult FormCreateLogin(UsersModel createUser)
        {
            createUser.CreateNewUser(createUser.UserName, createUser.Password, createUser.Balance, createUser.FirstName, createUser.LastName, createUser.SecurityAnswer);
            if (createUser.Message == "Created.")
            {
                return RedirectToAction("EntryIndex", "Entry");
            }
            else
            {
                return View("CreateLogin", createUser);
            }
        }
    }
}

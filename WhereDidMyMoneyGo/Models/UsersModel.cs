using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Collections.Generic;

namespace WhereDidMyMoneyGo.Models
{
    public class UsersModel
    {
        public UsersModel()
        {
            //When this Model is call, create a database object to use for calling the database with UserDapperRepo actions
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

            string connString = config.GetConnectionString("DefaultConnection");
            IDbConnection conn = new MySqlConnection(connString);

            RepoUser = new UsersDapperRepo(conn);
        }

        public UsersDapperRepo RepoUser { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string SecurityAnswer { get; set; }
        public string Balance { get; set; } //In database this is a double. Will start out as string here, then validate user input, and convert before sending to database
        public string Message { get; set; }

        //Get User
        public IEnumerable<UsersTable> GetUser(string userName)
        {
            return RepoUser.GetUserName(userName);
        }

        //Check if user exist, if so check password.
        public void Login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                Message = "Both fields must be filled in to Login.";
            }
            else
            {
                var userInfo = GetUser(userName);
                if (userInfo.Any())
                {
                    if (userInfo.First().Password == password)
                    {
                        Message = "Welcome.";

                    }
                    else
                    {
                        Message = "Password is incorrect.";
                    }
                }
                else
                {
                    Message = "Username does not exist.";
                }
            }
        }

        //Check if user exist, if so check security answer.
        public void GetPassword(string userName, string secureAns)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(secureAns))
            {
                Message = "Both fields must be filled in to Submit.";
            }
            else
            {
                var userInfo = GetUser(userName);
                if (userInfo.Any())
                {
                    if (userInfo.First().SecurityAnswer.ToLower() == secureAns.ToLower())
                    {
                        Message = $"Your password: {userInfo.First().Password}";
                    }
                    else
                    {
                        Message = "Security Answer is incorrect.";
                    }
                }
                else
                {
                    Message = "Username does not exist.";
                }
            }
        }

        //Check if user exist, if not check password rules, if good create user.
        public void CreateNewUser(string userName, string password, string balance, string first, string last, string secureAns)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(balance) || string.IsNullOrEmpty(first) || string.IsNullOrEmpty(last) || string.IsNullOrEmpty(secureAns))
            {
                Message = "All fields must be filled in to Submit.";
            }
            else
            {
                var userInfo = GetUser(userName);
                if (userInfo.Any())
                {
                    Message = "Username already exists.";
                }
                else
                {
                    if (password.Length >= 10 &&
                        password.Where(x => char.IsLetter(x)).Where(x => char.IsUpper(x)).Count() >= 1 &&
                        password.Where(x => char.IsLetter(x)).Where(x => char.IsLower(x)).Count() >= 1 &&
                        password.Where(x => char.IsNumber(x)).Count() >= 1)
                    {
                        var trimBalance = balance.Where(x => char.IsNumber(x) || x == '.');
                        int decimalPlaces = balance.Contains('.') ? balance.Substring(balance.IndexOf('.') + 1).Length : 0; 
                        if (trimBalance.Any() && trimBalance.Where(x => x == '.').Count() <= 1 && string.Join("",trimBalance) == balance && decimalPlaces <= 2 && Convert.ToDouble(balance) != 0)
                        {
                            RepoUser.InsertNewUser(userName, password, Convert.ToDouble(balance), first, last, secureAns);
                            Message = "Login successfully created.";
                        }
                        else
                        {
                            Message = "Beginning Balance must be numerical, non-negative, larger than zero, no more than 2 decimal places.";
                        }
                    }
                    else
                    {
                        Message = "Password must contain at least 10 characters, 1 uppercase letter, 1 lowercase letter, 1 number.";
                    }
                }
            }
        }

        //Update User Balance after Transaction or Adjustment
        public void UpdateBalance(int userId, double balance, double transAmount, string transType)
        {
            if (transType == "Revenue" || transType == "Adjustment Increase")
            {
                Balance = (balance + transAmount).ToString("0.00");
            }
            else
            {
                Balance = (balance - transAmount).ToString("0.00");
            }
            RepoUser.UpdateUserBalance(userId, Convert.ToDouble(Balance));
        }

        //Update User Password
        public void UpdatePassword(int userId, string passwordNew, string passwordOld)
        {
            if (string.IsNullOrEmpty(passwordNew))
            {
                Message = "Please fill in the corresponding field to create a new password.";
            }
            else
            {
                if (passwordNew != passwordOld)
                {
                    if (passwordNew.Length >= 10 &&
                        passwordNew.Where(x => char.IsLetter(x)).Where(x => char.IsUpper(x)).Count() >= 1 &&
                        passwordNew.Where(x => char.IsLetter(x)).Where(x => char.IsLower(x)).Count() >= 1 &&
                        passwordNew.Where(x => char.IsNumber(x)).Count() >= 1)
                    {
                        RepoUser.UpdateUserPassword(userId, passwordNew);
                        Message = "Your password has successfully been updated.";
                    }
                    else
                    {
                        Message = "Password must contain at least 10 characters, 1 uppercase letter, 1 lowercase letter, 1 number.";
                    }
                }
                else
                {
                    Message = "Please enter a password that is different from your current password.";
                }
            }
        }

        //Update User Security Answer
        public void UpdateSecurityAnswer(int userId, string secureAnsNew, string secureAnsOld)
        {
            if (string.IsNullOrEmpty(secureAnsNew))
            {
                Message = "Please fill in the corresponding field to update security answer.";
            }
            else
            {
                if (secureAnsNew == secureAnsOld)
                {
                    Message = "Please enter a security answer that is different from your current security answer.";
                }
                else
                {
                    RepoUser.UpdateUserSecuirtyAnswer(userId, secureAnsNew);
                    Message = "Your security answer has successfully been updated.";
                }
            }
        }
    }
}

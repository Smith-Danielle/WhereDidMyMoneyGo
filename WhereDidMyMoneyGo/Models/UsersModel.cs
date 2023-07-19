using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Linq;

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
        public double Balance { get; set; }
        public string Message { get; set; }

        public void Login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                Message = "Both fields must be filled in to Login.";
            }
            else
            {
                var userInfo = RepoUser.GetUserName(userName);
                if (userInfo.Select(x => x.UserName).Any())
                {
                    if (userInfo.Select(x => x.Password).First() == password)
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

        public void GetPassword(string userName, string secureAns)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(secureAns))
            {
                Message = "Both fields must be filled in to Submit.";
            }
            else
            {
                var userInfo = RepoUser.GetUserName(userName);
                if (userInfo.Select(x => x.UserName).Any())
                {
                    if (userInfo.Select(x => x.SecurityAnswer).First().ToLower() == secureAns.ToLower())
                    {
                        Message = $"Your password: {userInfo.Select(x => x.Password).First()}";
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
    }
}

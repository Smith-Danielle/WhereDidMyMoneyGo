using System;
using System.Data;
using Dapper;
using System.Collections.Generic;

namespace WhereDidMyMoneyGo.Models
{
    public class UsersDapperRepo
    {
        private readonly IDbConnection _connection;

        public UsersDapperRepo(IDbConnection connection)
        {
            _connection = connection;
        }

        //Get record based on inputted username
        public IEnumerable<UsersTable> GetUserName(string userName)
        {
            return _connection.Query<UsersTable>("Select * From Users Where UserName = @userName",
                   new { userName = userName });
        }

        //Insert new record
        public void InsertNewUser(string userName, string password, double balance, string first, string last, string secureAns)
        {
            _connection.Execute("Insert Into Users (Username, Password, Balance, FirstName, LastName, SecurityAnswer) Values (@userName, @password, @balance, @first, @last, @secureAns);",
            new { userName = userName, password = password, balance = balance, first = first, last = last, secureAns = secureAns });
        }
    }
}
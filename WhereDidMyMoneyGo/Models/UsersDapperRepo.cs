﻿using System;
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
            return _connection.Query<UsersTable>("Select * From Users Where UserName = @userName;",
                   new { userName = userName });
        }

        //Insert new record
        public void InsertNewUser(string userName, string password, double balance, string first, string last, string secureAns)
        {
            _connection.Execute("Insert Into Users (Username, Password, Balance, FirstName, LastName, SecurityAnswer) Values (@userName, @password, @balance, @first, @last, @secureAns);",
            new { userName = userName, password = password, balance = balance, first = first, last = last, secureAns = secureAns });
        }

        //Update record balance
        public void UpdateUserBalance(int userId, double balance)
        {
            _connection.Execute("Update Users Set Balance = @balance Where UserId = @userId;",
            new { userId = userId, balance = balance});
        }

        //Update record password
        public void UpdateUserPassword(int userId, string password)
        {
            _connection.Execute("Update Users Set Password = @password Where UserId = @userId;",
            new { userId = userId, password = password });
        }

        //Update record security answer
        public void UpdateUserSecuirtyAnswer(int userId, string secureAns)
        {
            _connection.Execute("Update Users Set SecurityAnswer = @secureAns Where UserId = @userId;",
            new { userId = userId, secureAns = secureAns });
        }
    }
}
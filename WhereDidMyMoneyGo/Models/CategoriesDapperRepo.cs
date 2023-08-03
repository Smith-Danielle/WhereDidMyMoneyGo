using System;
using System.Data;
using Dapper;
using System.Collections.Generic;

namespace WhereDidMyMoneyGo.Models
{
    public class CategoriesDapperRepo
    {
        private readonly IDbConnection _connection;

        public CategoriesDapperRepo(IDbConnection connection)
        {
            _connection = connection;
        }

        //Get all default category records and user's entered categories
        public IEnumerable<CategoriesTable> GetUserCats(int userId)
        {
            //Orders user's input then default by vendorname
            return _connection.Query<CategoriesTable>("Select * From Categories Where UserId = @userId Order By CategoryName;",
                   new { userId = userId });
        }

        //Insert new record
        public void InsertNewCategory(int userId, string categoryName)
        {
            _connection.Execute("Insert Into Categories (UserId, CategoryName) Values (@userId, @categoryName);",
            new { userId = userId, categoryName = categoryName });
        }

        //Insert Defaults for new Users
        public void InsertNewUserDefaultCats(int userId)
        {
            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values ('Deposit', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values('Misc. Revenue', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values('Rent', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values('Car Note', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values('Gas', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values('Food & Beverage', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values('Clothing', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values('Entertainment', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values('Misc. Expense', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Categories (CategoryName, UserId) Values('Balance Adjustment', @userId); ",
            new { userId = userId});
        }
    }
}

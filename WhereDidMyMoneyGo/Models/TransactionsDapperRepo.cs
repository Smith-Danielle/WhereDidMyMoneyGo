using System;
using System.Data;
using Dapper;
using System.Collections.Generic;

namespace WhereDidMyMoneyGo.Models
{
    public class TransactionsDapperRepo
    {
        private readonly IDbConnection _connection;

        public TransactionsDapperRepo(IDbConnection connection)
        {
            _connection = connection;
        }

        //Get all trans records for user
        public IEnumerable<TransactionsTable> GetUserTrans(int userId)
        {
            return _connection.Query<TransactionsTable>("Select t.transactiondate as TransactionDate, v.vendorname as VendorName, c.categoryname as CategoryName, t.transactiontype as TransactionType, t.transactionamount as TransactionAmount From transactions as t Inner Join vendors as v on t.vendorid = v.vendorid Inner Join categories as c on t.categoryid = c.categoryid Where t.userid = 1 Order By TransactionDate desc, VendorName, CategoryName, TransactionType, TransactionAmount;",
                   new { userId = userId});
        }

        //Insert new record
        public void InsertNewTrans(int userId, int venId, int catId, string date, string type, double amount)
        {
            _connection.Execute("Insert Into Transactions (UserId, VendorId, CategoryId, TransactionDate, TransactionType, TransactionAmount) Values (@userId, @venId, @catId, @date, @type, @amount);",
            new { userId = userId, venId = venId, catId = catId, date = date, type = type, amount = amount });
        }
    }
}

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
            return _connection.Query<TransactionsTable>("Select t.transactiondate as TransactionDate, v.vendorname as VendorName, c.categoryname as CategoryName, c.categorytype as CategoryType, t.transactionamount as TransactionAmount From transactions as t Inner Join vendors as v on t.vendorid = v.vendorid Inner Join categories as c on t.categoryid = c.categoryid Where t.userid = @userId Order By TransactionDate desc, VendorName, CategoryName, CategoryType, TransactionAmount;",
                   new { userId = userId});
        }
    }
}

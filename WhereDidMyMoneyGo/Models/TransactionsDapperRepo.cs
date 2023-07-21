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

        //Get top 10 records
        public IEnumerable<TransactionsTable> GetTopTrans()
        {
            //top 10 for a specific user, there will need to be a join for vendor and cat name
            return _connection.Query<TransactionsTable>("Select * From Transactions Limit 10;");
        }
    }
}

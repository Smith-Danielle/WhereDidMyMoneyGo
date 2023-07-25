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
        public IEnumerable<CategoriesTable> GetDefaultAndUserCats(int userId)
        {
            //Orders user's input then default by vendorname
            return _connection.Query<CategoriesTable>("Select * From Categories Where UserId In (1, @userId) and CategoryId != 17 Order By CategoryName;",
                   new { userId = userId });
        }
    }
}

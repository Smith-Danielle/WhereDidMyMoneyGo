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

        //Login Actions

        //Check input username to see if it exists, if so pull that entire record
        public IEnumerable<UsersTable> GetUserName(string userName)
        {
            return _connection.Query<UsersTable>("Select * From Users Where UserName = @userName",
                   new { userName = userName });
        }
    }
}

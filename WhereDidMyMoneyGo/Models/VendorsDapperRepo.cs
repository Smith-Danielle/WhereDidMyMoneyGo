using System;
using System.Data;
using Dapper;
using System.Collections.Generic;

namespace WhereDidMyMoneyGo.Models
{
    public class VendorsDapperRepo
    {
        private readonly IDbConnection _connection;

        public VendorsDapperRepo(IDbConnection connection)
        {
            _connection = connection;
        }

        //Get all default vendor records and user's entered vendors
        public IEnumerable<VendorsTable> GetDefaultAndUserVendors(int userId)
        {
            return _connection.Query<VendorsTable>("Select * From Vendors Where UserId In (1, @userId) and VendorId != 5 Order By VendorName;",
                   new { userId = userId });
        }

        //Insert new record
        public void InsertNewVendor(int userId, string vendorName)
        {
            _connection.Execute("Insert Into Vendors (UserId, VendorName) Values (@userId, @vendorName);",
            new { userId = userId, vendorName = vendorName });
        }

        
    }
}

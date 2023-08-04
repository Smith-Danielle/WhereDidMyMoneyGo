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
        public IEnumerable<VendorsTable> GetUserVendors(int userId)
        {
            return _connection.Query<VendorsTable>("Select * From Vendors Where UserId = @userId Order By VendorName;",
                   new { userId = userId });
        }

        //Insert new record
        public void InsertNewVendor(int userId, string vendorName)
        {
            _connection.Execute("Insert Into Vendors (UserId, VendorName) Values (@userId, @vendorName);",
            new { userId = userId, vendorName = vendorName });
        }

        //Delete record
        public void DeleteUserVendor(int userId, string vendorName)
        {
            _connection.Execute("Delete From Vendors Where UserId = @userId And VendorName = @vendorName;",
            new { userId = userId, vendorName = vendorName });
        }

        //Insert Defaults for new Users
        public void InsertNewUserDefaultVendors(int userId)
        {
            _connection.Execute("Insert Into Vendors (VendorName, UserId) Values ('Target', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Vendors(VendorName, UserId) Values('Walmart', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Vendors(VendorName, UserId) Values('Amazon', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Vendors(VendorName, UserId) Values('Employer', @userId);",
            new { userId = userId });

            _connection.Execute("Insert Into Vendors(VendorName, UserId) Values('User Adjustment', @userId);",
            new { userId = userId});
        }
    }
}

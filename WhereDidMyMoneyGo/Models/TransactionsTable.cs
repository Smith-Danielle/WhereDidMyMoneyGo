using System;
namespace WhereDidMyMoneyGo.Models
{
    public class TransactionsTable
    {
        public TransactionsTable()
        {
        }

        public int TransactionId { get; set; }
        public string TransactionDate { get; set; }
        public double TransactionAmount { get; set; }
        public int VendorId { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string VendorName { get; set; } //from Vendor table, join done in dapper repo, not needed for a transaction
        public string CategoryName { get; set; } //from Category table, join done in dapper repo, not needed for a transaction
        public string CategoryType { get; set; } //from Category table, join done in dapper repo, not needed for a transaction
    }
}

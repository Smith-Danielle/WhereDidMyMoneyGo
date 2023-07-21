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
    }
}

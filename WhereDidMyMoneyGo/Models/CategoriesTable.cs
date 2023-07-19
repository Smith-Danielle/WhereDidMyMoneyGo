using System;
namespace WhereDidMyMoneyGo.Models
{
    public class CategoriesTable
    {
        public CategoriesTable()
        {
        }

        public int CategoryId { get; set; }
        public int CategoryName { get; set; }
        public int CategoryType { get; set; } //Enum in database. Values: Revenue, Expense, Adjustment 
        public int UserId { get; set; }
    }
}

using System;
namespace WhereDidMyMoneyGo.Models
{
    public class CategoriesTable
    {
        public CategoriesTable()
        {
        }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int UserId { get; set; }
    }
}

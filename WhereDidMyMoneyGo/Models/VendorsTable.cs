using System;
namespace WhereDidMyMoneyGo.Models
{
    public class VendorsTable
    {
        public VendorsTable()
        {
        }

        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public int UserId { get; set; }
    }
}

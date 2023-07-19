using System;
namespace WhereDidMyMoneyGo.Models
{
    public class UsersTable
    {
        public UsersTable()
        {
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string SecurityAnswer { get; set; }
        public double Balance { get; set; }
    }
}

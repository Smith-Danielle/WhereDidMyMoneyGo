using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WhereDidMyMoneyGo.Models;

namespace WhereDidMyMoneyGo
{
    public class OverviewViewModel
    {
        public OverviewViewModel(UsersModel user, TransactionsModel trans)
        {
            OverUsersModel = user;
            OverTransactionModel = trans;
        }

        public UsersModel OverUsersModel { get; set; }
        public TransactionsModel OverTransactionModel { get; set; }
    }
}

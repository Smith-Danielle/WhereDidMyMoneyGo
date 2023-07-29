-- update money.users set securityanswer = 'Administration' where userid = 1 ;
-- update money.users set balance = 1000.00 where userid = 1;

-- insert into vendors (vendorname, userid) values ('Target', 1);
-- insert into vendors (vendorname, userid) values ('Walmart', 1);
-- insert into vendors (vendorname, userid) values ('Cinemark', 1);
-- insert into vendors (vendorname, userid) values ('Amazon', 1);

-- insert into transactions (transactiondate, transactionamount, vendorid, categoryid, userid) values ('2023-01-25', 25, 2, 13, 1);
-- insert into transactions (transactiondate, transactionamount, vendorid, categoryid, userid) values ('2023-03-07', 200, 3, 14, 1);
-- insert into transactions (transactiondate, transactionamount, vendorid, categoryid, userid) values ('2023-01-25', 135, 1, 15, 1);

-- Select statement for getting transactions in database. Joins to get the actual name vs ids.
/*
Select t.transactiondate as TransactionDate, v.vendorname as VendorName, c.categoryname as CategoryName, t.transactiontype as TranasctionType, t.transactionamount as TransactionAmount 
From transactions as t
Inner Join vendors as v on t.vendorid = v.vendorid 
Inner Join categories as c on t.categoryid = c.categoryid
Where t.userid = 1
Order By TransactionDate desc, VendorName, CategoryName, TranasctionType, TransactionAmount;
*/

-- Need Vendor in order to add User Adjustments
-- insert into vendors (VendorName, UserId) values ('User Adjustment', 1);

-- Gell all default vendors and vendors entered by user, exclude adjustment vendor id
-- Select * From Vendors Where UserId In (1, 2) and VendorId != 5 Order By VendorName;

-- Gell all default categories and categories entered by user, exclude adjustment category id
-- Select * From Categories Where UserId In (1, 2) and CategoryId != 17 Order By CategoryType, CategoryName;

-- Update TransactionType after table modifications
/*
update transactions set Transactiontype = 'Expense' where transactionid = 1;
update transactions set Transactiontype = 'Expense' where transactionid = 2;
update transactions set Transactiontype = 'Expense' where transactionid = 3;
*/

-- Clear transactions for more testing and to allow transaction type enum to be updated properly
-- delete from transactions;

select * from money.categories;
select * from money.vendors;
select * from money.transactions;
select * from money.users;
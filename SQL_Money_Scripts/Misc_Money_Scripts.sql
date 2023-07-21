-- update money.users set securityanswer = 'Administration' where userid = 1 ;
-- update money.users set balance = 1000.00 where userid = 1;

-- insert into vendors (vendorname, userid) values ('Target', 1);
-- insert into vendors (vendorname, userid) values ('Walmart', 1);
-- insert into vendors (vendorname, userid) values ('Cinemark', 1);
-- insert into vendors (vendorname, userid) values ('Amazon', 1);

-- insert into transactions (transactiondate, transactionamount, vendorid, categoryid, userid) values ('2023-01-25', 25, 2, 13, 1);
-- insert into transactions (transactiondate, transactionamount, vendorid, categoryid, userid) values ('2023-03-07', 200, 3, 14, 1);
-- insert into transactions (transactiondate, transactionamount, vendorid, categoryid, userid) values ('2023-01-25', 135, 1, 15, 1);

select * from money.categories;
select * from money.vendors;
select * from money.transactions;
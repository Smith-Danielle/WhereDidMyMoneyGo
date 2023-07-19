-- Admin User Creation
/*
Insert Into Money.Users (UserName, FirstName, LastName, Password) Values ('Admin', 'Admin', 'User', 'Admin*2023');
Select * From Money.Users;
*/

-- Insert Default Categories
/*
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Deposit', 'Revenue', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Misc. Revenue', 'Revenue', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Rent', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Electric', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Internet', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Misc. Utilities', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Car Note', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Car Insurance', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Car Maintenance', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Gas', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Medical', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Health Insurance', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Food & Beverage', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Entertainment', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Clothing', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Misc. Expense', 'Expense', 1);
Insert Into Money.Categories (CategoryName, CategoryType, UserId) Values ('Balance Adjustment', 'Adjustment', 1);
Select * From Money.Categories;
*/
ALTER TABLE [dbo].[Countries] 
    ADD [language] NVARCHAR (50);

create table dbo.ExpenseTypes(
	Id int identity (1,1) not null,
	nameKey nvarchar(20) not null,
	descriptionKey nvarchar(max) null,
	constraint PK_ExpenseTypes primary key clustered (Id asc)
	);

create table [dbo].[Expenses](
	Id int identity (1,1) not null,
	date datetime not null,
	amount int not null,
	currency_currencyCode nchar(3) null,
	expenseType_Id int not null constraint typeDef default 0,
	description nvarchar(MAX) not null constraint descDef default '',
	constraint PK_Expenses primary key clustered (Id asc),
	CONSTRAINT [FK_CurrencyExpense] FOREIGN KEY ([currency_currencyCode]) REFERENCES [dbo].[Currencies] ([currencyCode]),
	constraint FK_ExpenseTypeExpense foreign key (expenseType_Id) references dbo.ExpenseTypes (Id)
	);







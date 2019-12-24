CREATE NONCLUSTERED INDEX [IX_DATE]
ON [dbo].[Expenses]([date] ASC)

create nonclustered index [IX_DATE]
on [dbo].[ApartmentDays]([Apartment_Id] ASC, [date] ASC)

create nonclustered index [IX_DATE]
on [dbo].[EmployeeWorkDays]([dateAndTime] ASC)

alter table [dbo].[TranslationKeys]
alter column transKey nvarchar(100)

create nonclustered index [IX_transKey]
on [dbo].[TranslationKeys]([transKey] ASC)


create table [dbo].[Hotels] (
[Id]             INT            IDENTITY (1, 1) NOT NULL,
[nameKey]        nvarchar (MAX) not null,
[address] nvarchar(max)
constraint [Pk_hotels] primary key clustered ([Id] asc)
);
alter table [dbo].[apartments]
add [hotel_Id] int
CONSTRAINT [FK_HotelApartment] FOREIGN KEY ([hotel_Id]) REFERENCES [dbo].[Hotels] ([Id]);

alter table [dbo].[EmployeeWorkDays]
add [hotel_Id] int
constraint [FK_HotelEmployeeWorkDays] foreign key ([hotel_Id]) references [dbo].[Hotels] ([Id]);

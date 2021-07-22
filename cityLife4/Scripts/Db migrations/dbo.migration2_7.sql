create table Services (
Id int identity(1,1) not null,
name varchar(255) not null,
price decimal(6,2) not null,
constraint PK_Services primary key(Id)
);

create table Ordered_services (
Id int identity(1,1) not null,
order_date datetime,
amount int not null,
unit_price decimal (6,2) not null,
total_price decimal (6,2),
done bit not null,
order_id int not null,
service_id int not null,
constraint PK_Ordered_services primary key (Id),
constraint FK_Ordered_servies_orders foreign key (order_id) references Orders(Id),
constraint FK_Ordered_services_services foreign key (service_id) references Services(Id)
);

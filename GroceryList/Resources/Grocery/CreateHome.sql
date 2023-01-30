-- ensure collation is correct

-- create current list table
create table `{{homeId}}grocery_list_current` (
    `id` int not null primary key autoincrement,
    `home_id` varchar(50) not null,
    `name` varchar(50) not null,
    `brand` ,
    `notes` ,
    `price` double,
    `qty` int,
    `created_time` timestamp not null,
    `created_user` varchar(100) not null,
    `in_cart_time` timestamp,
    `in_cart_user` varchar(100),
    `purchased_time` timestamp,
    `purchased_user` varchar(100)
);

-- create trips table(s)
create table `{{homeId}}grocery_list_trips` (
);

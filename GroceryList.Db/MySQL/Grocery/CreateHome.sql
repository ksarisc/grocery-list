-- ensure collation is correct

-- create current list table
-- create table `{{homeId}}grocery_list_current` (
--     `id` INT not null primary key autoincrement,
--     `home_id` INT not null,
--     `name` varchar(50) not null,
--     `brand` ,
--     `notes` ,
--     `price` double,
--     `qty` int,
--     `created_time` timestamp not null,
--     `created_user` varchar(100) not null,
--     `in_cart_time` timestamp,
--     `in_cart_user` varchar(100),
--     `purchased_time` timestamp,
--     `purchased_user` varchar(100)
-- );
-- -- create trips table(s)
-- create table `{{homeId}}grocery_list_trips` (
-- );
CREATE TABLE IF NOT EXISTS `{{homeId}}_current_list` (
    `item_id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `home_id` INT NOT NULL,
    `name` varchar(50) NOT NULL,
    `section` varchar(50),
    `brand` varchar(50),
    `notes` varchar(1000),
    `price` double,
    `qty` INT,
    `created_on` datetime,
    `created_tz` varchar(40),
    `created_user` varchar(50) NOT NULL,
    `in_cart_on` datetime,
    `in_cart_tz` varchar(40),
    `in_cart_user` varchar(50),
    `purchased_on` datetime,
    `purchased_tz` varchar(40),
    `purchased_user` varchar(50)
) DEFAULT CHARSET=utf8mb4;

-- create the previous trips table
CREATE TABLE IF NOT EXISTS `{{homeId}}_previous_trips` (
    `trip_id` BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `checkout_on` datetime,
    `checkout_tz` varchar(40),
    `item_list` JSON NOT NULL,
    `store_name` varchar(100),
    `total` double
) DEFAULT CHARSET=utf8mb4;

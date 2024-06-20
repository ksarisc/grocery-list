-- ensure collation is correct

-- create current list table
-- create table `{{homeId}}grocery_list_current` (
--     `id` INT not null primary key autoincrement,
--     `home_id` INT not null,
--     `name` VARCHAR(50) not null,
--     `brand` ,
--     `notes` ,
--     `price` double,
--     `qty` int,
--     `created_time` timestamp not null,
--     `created_user` VARCHAR(100) not null,
--     `in_cart_time` timestamp,
--     `in_cart_user` VARCHAR(100),
--     `purchased_time` timestamp,
--     `purchased_user` VARCHAR(100)
-- );
-- -- create trips table(s)
-- create table `{{homeId}}grocery_list_trips` (
-- );
CREATE TABLE IF NOT EXISTS `{{homeId}}_current_list` (
    `item_id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `home_id` INT NOT NULL,
    `name` VARCHAR(50) NOT NULL,
    `section` VARCHAR(50),
    `brand` VARCHAR(50),
    `notes` VARCHAR(1000),
    `price` double,
    `qty` INT,
    `created_on` datetime NOT NULL,
    `created_tz` VARCHAR(40) NOT NULL,
    `created_user` VARCHAR(50) NOT NULL,
    `in_cart_on` datetime,
    `in_cart_tz` VARCHAR(40),
    `in_cart_user` VARCHAR(50),
    `purchased_on` datetime,
    `purchased_tz` VARCHAR(40),
    `purchased_user` VARCHAR(50)
) DEFAULT CHARSET=utf8mb4;

-- create the previous trips table
CREATE TABLE IF NOT EXISTS `{{homeId}}_previous_trips` (
    `trip_id` BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `checkout_on` datetime,
    `checkout_tz` VARCHAR(40),
    `item_list` JSON NOT NULL,
    `store_name` VARCHAR(100),
    `total` double
) DEFAULT CHARSET=utf8mb4;

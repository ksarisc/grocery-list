-- create the "database"
-- setup the connection (outside)

-- USE db; -- TABLE_SCHEMA LIKE 'dbo'
IF EXISTS(SELECT * FROM information_schema.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME IN('{0}_current_trip', '{0}_previous_trips'))
BEGIN
	exit;
END;

-- create the current list table
CREATE TABLE `{0}_current_trip` (
    `item_id` uint not null autoincrement primary key,
    `home_id` uint not null,
    `name` varchar(50) NOT NULL,
    `section` varchar(50),
    `brand` varchar(50),
    `notes` varchar(1000),
    `price` double,
    `qty` int,
    `created_at` datetime,
    `created_tz` varchar(40),
    `created_user` varchar(50) NOT NULL,
    `in_cart_at` datetime,
    `in_cart_tz` varchar(40),
    `in_cart_user` varchar(50),
    `purchased_at` datetime,
    `purchased_tz` varchar(40),
    `purchased_user` varchar(50)
) DEFAULT CHARSET=utf8mb4;


-- create the previous trips table
CREATE TABLE `{0}_previous_trips` (
    -- `trip_id` uint64 not null autoincrement primary key,
    `checkout_at` datetime primary key,
    `checkout_tz` varchar(40),
	`item_list` JSON NOT NULL,
    `store_name` varchar(100),
    `total` double
) DEFAULT CHARSET=utf8mb4;

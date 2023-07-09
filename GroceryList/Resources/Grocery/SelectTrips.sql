SELECT *
-- `trip_id` BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
-- `checkout_at` datetime,
-- `checkout_tz` varchar(40),
-- `item_list` JSON NOT NULL,
-- `store_name` varchar(100),
-- `total` double
FROM `{{homeId}}_previous_trips`

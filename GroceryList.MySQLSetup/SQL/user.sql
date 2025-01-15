CREATE TABLE `user` (
	`id` int unsigned NOT NULL AUTO_INCREMENT,
	`name` varchar(100) NOT NULL,
	`email` varchar(200) NOT NULL,
	`pass_hash` varbinary(512) NOT NULL,
	`auth_ep` varchar(400),
	`last_token` blob,
	`last_logged_in` datetime,
	`created_on` datetime NOT NULL,
	`created_by` varchar(100) NOT NULL,
	`modified_on` datetime,
	`modified_by` varchar(100),
	`active` BIT(1) NOT NULL DEFAULT 0
	PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE utf8_general_ci;

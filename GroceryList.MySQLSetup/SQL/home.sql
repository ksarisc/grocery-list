CREATE TABLE `home` (
	`id` int unsigned NOT NULL AUTO_INCREMENT,
	`slug` varchar(50) NOT NULL,
	`label` varchar(100) NOT NULL,
	-- datetimeoffset NOT an option so this is UTC
	`created_on` datetime NOT NULL,
	`created_by` varchar(200) NOT NULL,
	`created_meta` varchar(1000) NOT NULL,
	PRIMARY KEY (`slug`) -- id
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE utf8_general_ci;

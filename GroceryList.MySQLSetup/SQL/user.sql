CREATE TABLE `user` (
	`id` uint  NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(50) NOT NULL,
	`email` VARCHAR(200) NOT NULL,
	`pass_hash` VARBINARY(512) NOT NULL,
	`auth_ep` VARCHAR(400),
	`last_token` blob,
	`last_logged_in` datetimeoffset,
	-- datetimeoffset NOT an option

	`created_on` datetimeoffset NOT NULL,
	`created_by` VARCHAR(50) NOT NULL,
	`modified_on` datetimeoffset,
	`modified_by` VARCHAR(50),
	`active` BIT NOT NULL DEFAULT 0
	PRIMARY KEY (`id`)
);

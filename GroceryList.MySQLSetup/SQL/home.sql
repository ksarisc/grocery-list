CREATE TABLE `home` (
	`id` uint  NOT NULL AUTO_INCREMENT,
	`label` VARCHAR(30) NOT NULL,
	-- datetimeoffset NOT an option
	`created_on` datetimeoffset NOT NULL,
	`created_by` VARCHAR(50) NOT NULL,
	PRIMARY KEY (`id`)
);

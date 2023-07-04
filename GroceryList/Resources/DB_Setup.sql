-- Homes
CREATE TABLE IF NOT EXISTS `homes` (
	`home_id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`identifier` VARCHAR(50) NOT NULL, -- ULID/GUID/UUID/Other generally based on home_id
	`name` VARCHAR(100),
	`description` VARCHAR(2000),
	`primary_user` VARCHAR(50)
) DEFAULT CHARSET=utf8mb4;

-- Users
CREATE TABLE IF NOT EXISTS `users` (
	`user_id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`user_name` VARCHAR(50) NOT NULL,
	`email_address` VARCHAR(200) NOT NULL,
	`login_context` VARCHAR(4000) NOT NULL,
	`secret` VARCHAR(100) NOT NULL
);

-- Login Events
CREATE TABLE IF NOT EXISTS `users_events` (
	`user_id` INT NOT NULL,
	`event_time` DATETIME NOT NULL,
	`context` VARCHAR(4000) NOT NULL,
	PRIMARY KEY(`user_id`, `event_time`)
);

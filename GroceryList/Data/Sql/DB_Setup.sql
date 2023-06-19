-- Homes
CREATE TABLE IF NOT EXISTS `homes` (
	`home_id` uint not null AUTO_INCREMENT primary key,
	`identifier` varchar(50) not null, -- ULID/GUID/UUID/Other generally based on home_id
	`name` varchar(100),
	`description` varchar(2000),
	`primary_user` varchar(50)
) DEFAULT CHARSET=utf8mb4;

-- Users
CREATE TABLE IF NOT EXISTS `users` (
	`user_id` uint not null AUTO_INCREMENT primary key,
	`user_name` varchar(50) not null,
	`email_address` varchar(200) not null,
	`login_context` varchar(4000) not null,
	`secret` varchar(100) not null
);

-- Login Events

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
	/*
`user_name` -- UserName
`user_name_normal` -- NormalizedUserName
`email_address` -- Email
`email_address_normal` -- NormalizedEmail
`email_address_confirmed` -- EmailConfirmed
`password_hash` -- PasswordHash
-- PhoneNumber
-- PhoneNumberConfirmed
`two_factor_enabled` -- TwoFactorEnabled
	*/
	`secret` VARCHAR(100) NOT NULL
);

-- Login Events
CREATE TABLE IF NOT EXISTS `users_events` (
	`user_id` INT NOT NULL,
	`event_time` DATETIME NOT NULL,
	`context` VARCHAR(4000) NOT NULL,
	PRIMARY KEY(`user_id`, `event_time`)
);

CREATE TABLE IF NOT EXISTS `roles` (
	`role_id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`role_name` VARCHAR(50) NOT NULL,
	/*
	public string Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? EditedTime { get; set; }
	*/
);
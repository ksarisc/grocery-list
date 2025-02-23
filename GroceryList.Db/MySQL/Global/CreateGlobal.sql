CREATE TABLE IF NOT EXISTS `home` (
-- `home_id`, `identifier`, `name`, `created_by`, `created_time`, `created_by_meta`, `description`, `primary_user`
    `home_id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `slug` VARCHAR(50) NOT NULL,
    `label` VARCHAR(50) NOT NULL,
    `email` VARCHAR(200) NOT NULL,
    `timezone` VARCHAR(100) NOT NULL,
    `notes` VARCHAR(1000),
    `created_by_meta` VARCHAR(1000),
    `created_on` datetime NOT NULL,
    `created_by` VARCHAR(50) NOT NULL
) DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `home_security` (
    `id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `home_id` INT NOT NULL,
    `details` JSON NOT NULL,
    `created_on` datetime NOT NULL,
    `created_user` VARCHAR(50) NOT NULL
) DEFAULT CHARSET=utf8mb4;

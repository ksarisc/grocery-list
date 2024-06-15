-- store `home_id` in table, when it's already in the table name?
SELECT `item_id`, `name`, `section`, `brand`, `notes`, `price`, `qty`,
	`created_on`, `created_tz`, `created_user`,
	`in_cart_on`, `in_cart_tz`, `in_cart_user`,
	`purchased_on`, `purchased_tz`, `purchased_user`
FROM `{{homeId}}_current_list`
-- WHERE `home_id` = @HomeId
;
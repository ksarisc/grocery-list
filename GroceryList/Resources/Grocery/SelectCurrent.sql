SELECT `id` Id, `home_id` HomeId, `name` Name, `brand` Brand, `notes` Notes, `price` Price,
    `qty` Qty, `created_time` CreatedTime, `created_user` CreatedUser, `in_cart_time` InCartTime,
    `in_cart_user` InCartUser, `purchased_time` PurchasedTime, `purchased_user` PurchasedUser
FROM `{{homeId}}grocery_list_current`
WHERE /**where**/

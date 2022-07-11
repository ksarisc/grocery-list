IF @Id IS NOT NULL AND EXISTS(SELECT * FROM `{{homeId}}grocery_list_current` WHERE `id` = @Id)
BEGIN
    UPDATE `{{homeId}}grocery_list_current`
    SET `name` = @Name, `brand` = @Brand, `notes` = @Notes, `price` = @Price, `qty` = @Qty,
        `in_cart_time` = @InCartTime, `in_cart_user` = @InCartUser,
        `purchased_time` = @PurchasedTime, `purchased_user` = @PurchasedUser
    WHERE `id` = @Id -- AND `home_id` = @HomeId;
    -- ?? validate the home ID is correct ??
END
ELSE
    INSERT INTO `{{homeId}}grocery_list_current` (
        -- `id`,
        `home_id`, `name`, `brand`, `notes`, `price`,
        `qty`, `created_time`, `created_user`, `in_cart_time`,
        `in_cart_user`, `purchased_time`, `purchased_user`
    ) VALUES (
        -- @Id,
        @HomeId, @Name, @Brand, @Notes, @Price,
        @Qty, @CreatedTime, @CreatedUser, @InCartTime,
        @InCartUser, @PurchasedTime, @PurchasedUser
    );

    SET @Id = LAST_INSERT_ID();
BEGIN
END;

-- SELECT RESOURCE FILE WILL BE INSERTED HERE INSTEAD
{{SelectQuery}}
WHERE `id` = @Id;

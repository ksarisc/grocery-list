-- make trip from list of Ids for given home
INSERT INTO `{{homeId}}_previous_trips` (`checkout_at`, `checkout_tz`, `item_list`, `store_name`, `total`)
SELECT @CheckoutAt, @CheckoutTZ, item_list, @StoreName, @Total
FROM `{{homeId}}_current_list`
WHERE `id` IN({{idList}});


-- delete Ids from current once saved
DELETE
FROM `{{homeId}}_current_list`
WHERE `id` IN({{idList}});

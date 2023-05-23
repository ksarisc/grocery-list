-- return the JSON blobs in this searchable results?
SELECT `checkout_at`, `checkout_tz`, `item_list`, `store_name`, `total`
FROM `{0}_previous_trips`
WHERE `checkout_at` BETWEEN @CheckoutStart AND @CheckoutEnd
;
-- return the JSON blobs in this searchable results? -- `item_list`, 
SELECT `trip_id`, `checkout_at`, `checkout_tz`, `store_name`, `total`
FROM `{{homeId}}_previous_trips`
WHERE `checkout_at` BETWEEN @CheckoutStart AND @CheckoutEnd
;
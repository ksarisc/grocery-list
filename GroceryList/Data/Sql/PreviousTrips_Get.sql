-- return the JSON blobs in this searchable results? -- `item_list`, 
SELECT `trip_id`, `checkout_on`, `checkout_tz`, `store_name`, `total`
FROM `{{homeId}}_previous_trips`
WHERE `checkout_on` BETWEEN @CheckoutStart AND @CheckoutEnd
;
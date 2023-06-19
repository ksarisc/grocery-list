SELECT `item_list` -- `checkout_at`, `checkout_tz`, `store_name`, `total`
FROM `{{homeId}}_previous_trips`
WHERE `trip_id` = @TripId
;
SELECT `item_list` -- `checkout_on`, `checkout_tz`, `store_name`, `total`
FROM `{{homeId}}_previous_trips`
WHERE `trip_id` = @TripId
;
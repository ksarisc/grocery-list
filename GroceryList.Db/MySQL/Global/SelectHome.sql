SELECT
    `home_id` AS `Id`,
    `label` AS `Title`,
    `created_user` AS `CreatedBy`,
    `created_on` AS `CreatedTime`,
    `created_user` AS `CreatedByMeta`
    `email` AS `Contact`,
    `timezone` AS `TimeZone`,
    `notes` AS `Notes`
FROM `home`
WHERE `slug` = @HomeSlug;

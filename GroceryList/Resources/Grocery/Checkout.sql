-- make trip from list of Ids for given home
INSERT INTO `{{homeId}}grocery_list_current`
WHERE `id` = @Id;


-- delete Ids from current once saved
DELETE
FROM `{{homeId}}grocery_list_current`
WHERE `id` = @Id;

-- create the "database"
-- setup the connection (outside)

-- USE db; -- TABLE_SCHEMA LIKE 'dbo'
IF EXISTS(SELECT * FROM information_schema.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME IN('{0}_current_trip', '{0}_previous_trips'))
BEGIN
	exit;
END;

-- create the current list table
CREATE TABLE `{0}_current_trip` (
    -- [StringLength(50, MinimumLength = 20)]
    trip_id uint not null autoincrement,
    -- [StringLength(50, MinimumLength = 20)]
    HomeId uint not null,
    -- [StringLength(50, MinimumLength = 2)]
    Name varchar(50) NOT NULL,
    -- [StringLength(50, MinimumLength = 2)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Section { get; set; }
    -- [StringLength(50, MinimumLength = 2)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Brand { get; set; }

    -- [StringLength(1000, MinimumLength = 4)]
        public string? Notes

        public double? Price
    -- ?? assume NULL is 1 ??
        public int? Qty

        [Required]
        public DateTimeOffset CreatedTime { get; set; }
        [Required]
        public string CreatedUser { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? InCartTime { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? InCartUser { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? PurchasedTime { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PurchasedUser { get; set; }

	checkout_at datetimeoffset NOT NULL,
	item_list nvarchar(MAX) NOT NULL,
);


-- create the previous trips table
CREATE TABLE `{0}_previous_trips` (
	checkout_at datetimeoffset NOT NULL,
	item_list nvarchar(MAX) NOT NULL,
);

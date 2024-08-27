DECLARE @text_type nvarchar(50) = 'text';
DECLARE @textarea_type nvarchar(50) = 'textarea';
DECLARE @select nvarchar(50) = 'select';
DECLARE @int nvarchar(50) = 'number';

DECLARE @kafka_schema_type nvarchar(50) = 'EVENT_TYPE_KAFKA';
DECLARE @kafka_schema_name nvarchar(255) = 'Event Forwarding Kafka Schema';

DECLARE @kafka_host_key nvarchar(50) = 'kafka_host';
DECLARE @kafka_topic_key nvarchar(50) = 'kafka_topic';
DECLARE @kafka_partition_key nvarchar(50) = 'kafka_partition';
DECLARE @kafka_authentication_key nvarchar(50) = 'kafka_authentication';
DECLARE @kafka_username_key nvarchar(50) = 'kafka_username';
DECLARE @kafka_password_key nvarchar(50) = 'kafka_password';
DECLARE @kafka_payload_key nvarchar(50) = 'kafka_payload';
DECLARE @payload_regex nvarchar(255) = '^(([^=]+=.*)?\n?)*$';

DECLARE @kafka_authentication_sha512 nvarchar(50) = 'ScramSha512';

DECLARE @schema_id uniqueidentifier;
DECLARE @key nvarchar(50);
DECLARE @name nvarchar(255);
DECLARE @place_holder nvarchar(2048);
DECLARE @schema_detail_id nvarchar(50);
DECLARE @max_value int = 2147483647;
DECLARE @min_value int = 0;
-- Insert Schema
IF NOT EXISTS (SELECT 1 FROM [schemas] WITH (NOLOCK) WHERE [type] = @kafka_schema_type)
BEGIN
	insert into [schemas]([type], [name]) values (@kafka_schema_type, @kafka_schema_name)
END
ELSE
BEGIN
	UPDATE [schemas] SET [name] = @kafka_schema_name WHERE [type] = @kafka_schema_type
END
-- Insert Schema Detail
-- Insert Schema Detail for Kafka
SELECT @schema_id = [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @kafka_schema_type;

SET @key = @kafka_host_key;
SET @name = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_HOST.NAME';
SET @place_holder = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_HOST.PLACE_HOLDER';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value] , [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @text_type, NULL, NULL, NULL, NULL ,1)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @text_type, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 1
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @kafka_topic_key;
SET @name = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_TOPIC.NAME';
SET @place_holder = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_TOPIC.PLACE_HOLDER';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @text_type, NULL, NULL, NULL, NULL, 2)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @text_type, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 2
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @kafka_partition_key;
SET @name = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_PARTITION.NAME';
SET @place_holder = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_PARTITION.PLACE_HOLDER';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 0, 0
    , @place_holder, @int, NULL, @min_value, @max_value, NULL, 3)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 0, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @int, [regex] = NULL
    , [min_value] = @min_value, [max_value] = @max_value, [default_value] = NULL, [order] = 3
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @kafka_authentication_key;
SET @name = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_AUTHENTICATION.NAME';
SET @place_holder = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_AUTHENTICATION.PLACE_HOLDER';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder,@select, NULL, NULL, NULL, @kafka_authentication_sha512, 4)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @select, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = @kafka_authentication_sha512, [order] = 4
    WHERE [key] = @key and [schema_id] = @schema_id
END


SET @key = @kafka_username_key;
SET @name = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_USERNAME.NAME';
SET @place_holder = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_USERNAME.PLACE_HOLDER';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @text_type, NULL, NULL, NULL, NULL, 5)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @text_type, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 5
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @kafka_password_key;
SET @name = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_PASSWORD.NAME';
SET @place_holder = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_PASSWORD.PLACE_HOLDER';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @text_type, NULL, NULL, NULL, NULL, 6)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @text_type, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 6
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @kafka_payload_key;
SET @name = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_PAYLOAD.NAME';
SET @place_holder = 'BROKER.SCHEMA.EVENT_TYPE_KAFKA.KAFKA_PAYLOAD.PLACE_HOLDER';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 0, 0
    , @place_holder, @textarea_type, @payload_regex, NULL, NULL, NULL, 7)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 0, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @textarea_type, [regex] = @payload_regex
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 7
    WHERE [key] = @key and [schema_id] = @schema_id
END

--Insert into Schema Detail Option
SELECT @schema_detail_id = [id] FROM [schema_details] WITH(NOLOCK) WHERE [key] = @kafka_authentication_key;

SET @key = @kafka_authentication_sha512;

IF NOT EXISTS (SELECT 1 FROM [schema_detail_options] WITH (NOLOCK) WHERE [id] = @key and [schema_detail_id] = @schema_detail_id)
BEGIN
	INSERT INTO [schema_detail_options]([id], [name], [schema_detail_id]
    , [order],[code])
    VALUES (@key, @key, @schema_detail_id, 1, @key)
END
ELSE
BEGIN
	UPDATE [schema_detail_options] SET [name] = @key,[order] = 1,[code] = @key
    WHERE [id] = @key and [schema_detail_id] = @schema_detail_id
END

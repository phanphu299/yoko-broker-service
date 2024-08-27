DECLARE @text_type nvarchar(50) = 'text';
DECLARE @textarea_type nvarchar(50) = 'textarea';

DECLARE @rabbitmq_schema_type nvarchar(50) = 'EVENT_TYPE_RABBITMQ';
DECLARE @rabbitmq_schema_name nvarchar(255) = 'Event Forwarding Rabbit MQ Schema';
DECLARE @event_hub_schema_type nvarchar(50) = 'EVENT_TYPE_AZURE_EVENT_HUB';
DECLARE @event_hub_schema_name nvarchar(255) = 'Event Forwarding Azure Event Hub Schema';
DECLARE @webhook_schema_type nvarchar(50) = 'EVENT_TYPE_WEBHOOK';
DECLARE @webhook_schema_name nvarchar(255) = 'Event Forwarding Webhook Schema';

DECLARE @rabbitmq_exchange_topic_key nvarchar(50) = 'rabbitmq_exchange_topic';
DECLARE @rabbitmq_routing_key_key nvarchar(50) = 'rabbitmq_routing_key';
DECLARE @rabbitmq_host_key nvarchar(50) = 'rabbitmq_host';
DECLARE @rabbitmq_username_key nvarchar(50) = 'rabbitmq_username';
DECLARE @rabbitmq_password_key nvarchar(50) = 'rabbitmq_password';
DECLARE @rabbitmq_payload_key nvarchar(50) = 'rabbitmq_payload';
DECLARE @event_hub_connection_string_key nvarchar(50) = 'event_hub_connection_string';
DECLARE @event_hub_name_key nvarchar(50) = 'event_hub_name';
DECLARE @event_hub_payload_key nvarchar(50) = 'event_hub_payload';

DECLARE @event_hub_name_regex nvarchar(255) = '^[a-zA-Z]+[a-z0-9-_]*$';
DECLARE @payload_regex nvarchar(255) = '^(([^=]+=.*)?\n?)*$';

DECLARE @webhook_endpoint_key nvarchar(50) = 'webhook_endpoint';
DECLARE @webhook_payload_key nvarchar(50) = 'webhook_payload';

DECLARE @schema_id uniqueidentifier;
DECLARE @key nvarchar(50);
DECLARE @name nvarchar(255);
DECLARE @place_holder nvarchar(2048);

-- Insert Schema
IF NOT EXISTS (SELECT 1 FROM [schemas] WITH (NOLOCK) WHERE [type] = @rabbitmq_schema_type)
BEGIN
	insert into [schemas]([type], [name]) values (@rabbitmq_schema_type, @rabbitmq_schema_name)
END
ELSE
BEGIN
	UPDATE [schemas] SET [name] = @rabbitmq_schema_name WHERE [type] = @rabbitmq_schema_type
END

IF NOT EXISTS (SELECT 1 FROM [schemas] WITH (NOLOCK) WHERE [type] = @event_hub_schema_type)
BEGIN
	insert into [schemas]([type], [name]) values (@event_hub_schema_type, @event_hub_schema_name)
END
ELSE
BEGIN
	UPDATE [schemas] SET [name] = @event_hub_schema_name WHERE [type] = @event_hub_schema_type
END

IF NOT EXISTS (SELECT 1 FROM [schemas] WITH (NOLOCK) WHERE [type] = @webhook_schema_type)
BEGIN
	insert into [schemas]([type], [name]) values (@webhook_schema_type, @webhook_schema_name)
END
ELSE
BEGIN
	UPDATE [schemas] SET [name] = @webhook_schema_name WHERE [type] = @webhook_schema_type
END

-- Insert Schema Detail
-- Insert Schema Detail for RabbitMQ
SELECT @schema_id = [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @rabbitmq_schema_type;

SET @key = @rabbitmq_exchange_topic_key;
SET @name = 'Exchange Topic';
SET @place_holder = 'Exchange Topic';
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

SET @key = @rabbitmq_routing_key_key;
SET @name = 'Routing Key';
SET @place_holder = 'all';
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

SET @key = @rabbitmq_host_key;
SET @name = 'Host';
SET @place_holder = 'rabbitmq ip or dns name';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @text_type, NULL, NULL, NULL, NULL, 3)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @text_type, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 3
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @rabbitmq_username_key;
SET @name = 'Username';
SET @place_holder = 'rabbitmq username';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @text_type, NULL, NULL, NULL, NULL, 4)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @text_type, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 4
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @rabbitmq_password_key;
SET @name = 'Password';
SET @place_holder = 'rabbitmq password';
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

SET @key = @rabbitmq_payload_key;
SET @name = 'Payload';
SET @place_holder = NULL;
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 0, 0
    , @place_holder, @textarea_type, @payload_regex, NULL, NULL, NULL, 6)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 0, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @textarea_type, [regex] = @payload_regex
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 6
    WHERE [key] = @key and [schema_id] = @schema_id
END

-- Insert Schema Detail for Azure Event Hub
SELECT @schema_id = [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @event_hub_schema_type;

SET @key = @event_hub_connection_string_key;
SET @name = 'Connection String';
SET @place_holder = 'eventhub connectionstring';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @text_type, NULL, NULL, NULL, NULL, 7)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @text_type, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 7
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @event_hub_name_key;
SET @name = 'Hub Name';
SET @place_holder = 'event hub name';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @text_type, @event_hub_name_regex, NULL, NULL, NULL, 8)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @text_type, [regex] = @event_hub_name_regex
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 8
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @event_hub_payload_key;
SET @name = 'Payload';
SET @place_holder = NULL;
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 0, 0
    , @place_holder, @textarea_type, @payload_regex, NULL, NULL, NULL, 9)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 0, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @textarea_type, [regex] = @payload_regex
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 9
    WHERE [key] = @key and [schema_id] = @schema_id
END

-- Insert Schema Detail for Webhook
SELECT @schema_id = [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @webhook_schema_type;

SET @key = @webhook_endpoint_key;
SET @name = 'Endpoint';
SET @place_holder = 'Endpoint';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @text_type, NULL, NULL, NULL, NULL, 10)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @text_type, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 10
    WHERE [key] = @key and [schema_id] = @schema_id
END

SET @key = @webhook_payload_key;
SET @name = 'Payload';
SET @place_holder = 'Payload';
IF NOT EXISTS (SELECT 1 FROM [schema_details] WITH (NOLOCK) WHERE [key] = @key and [schema_id] = @schema_id)
BEGIN
	INSERT INTO [schema_details]([schema_id], [key], [name], [is_required], [is_readonly]
    , [place_holder], [data_type], [regex], [min_value], [max_value], [default_value], [order])
    VALUES (@schema_id, @key, @name, 1, 0
    , @place_holder, @textarea_type, NULL, NULL, NULL, NULL, 10)
END
ELSE
BEGIN
	UPDATE [schema_details] SET [name] = @name, [is_required] = 1, [is_readonly] = 0
    , [place_holder] = @place_holder, [data_type] = @textarea_type, [regex] = NULL
    , [min_value] = NULL, [max_value] = NULL, [default_value] = NULL, [order] = 10
    WHERE [key] = @key and [schema_id] = @schema_id
END
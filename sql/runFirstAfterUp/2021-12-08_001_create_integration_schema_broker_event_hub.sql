DECLARE @select_type nvarchar(50) = 'select';
DECLARE @text_type nvarchar(50) = 'text';
DECLARE @number_type nvarchar(50) = 'number';
DECLARE @bool_type nvarchar(50) = 'bool';

DECLARE @schema_type nvarchar(50) = 'BROKER_EVENT_HUB';
DECLARE @schema_name nvarchar(255) = 'Broker Event Hub Schema';

DECLARE @tier_key nvarchar(50) = 'tier';
DECLARE @hub_name_key nvarchar(50) = 'event_hub_name';
DECLARE @throughput_key nvarchar(50) = 'throughput_units';
DECLARE @auto_inflate_key nvarchar(50) = 'auto_inflate';
DECLARE @max_throughput_key nvarchar(50) = 'max_throughput_units';
DECLARE @connection_string_key nvarchar(50) = 'connection_string';
DECLARE @sas_token_key nvarchar(50) = 'sasToken';
DECLARE @sas_token_duration_key nvarchar(50) = 'sasTokenDuration';

DECLARE @hub_name_regex nvarchar(255) = '^[a-zA-Z]+[a-z0-9-_]*$';

DECLARE @schema_id uniqueidentifier;
DECLARE @detail_id uniqueidentifier;

-- Remove old
DELETE FROM [schema_detail_options] WHERE [schema_detail_id] IN (SELECT [id] FROM [schema_details] WITH(NOLOCK) WHERE [schema_id] IN (SELECT [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @schema_type));
DELETE FROM [schema_details] WHERE [schema_id] IN (SELECT [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @schema_type);
DELETE FROM [schemas] WHERE [type] = @schema_type;

-- Insert Schema
INSERT INTO [schemas]([name], [type])
VALUES (@schema_name, @schema_type);

-- Insert Schema Detail
SELECT @schema_id = [id] FROM [schemas] WHERE [type] = @schema_type;

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [default_value] , [order])
VALUES (@schema_id, @tier_key, 'Tier', 'Select Tier', @select_type, 1 ,'Basic', 1);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [regex],[default_value] , [order], [enable_copy])
VALUES (@schema_id, @hub_name_key, 'Hub Name', 'Hub Name', @text_type, 1, @hub_name_regex,'ingestion', 2 ,1);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [min_value], [max_value],[default_value] , [order])
VALUES (@schema_id, @throughput_key, 'Throughput Units', 'Throughput Units', @number_type, 1, 1, 20,1, 3);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [order],[default_value])
VALUES (@schema_id, @auto_inflate_key, 'Auto-inflate', 'Auto-inflate', @bool_type, 1,4, 'false');

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [min_value], [max_value],[default_value] , [order])
VALUES (@schema_id, @max_throughput_key, 'Maximum Throughput Units', 'Maximum Throughput Units', @number_type, 1, 1, 20,1,4);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [min_value], [max_value],[default_value] , [order])
VALUES (@schema_id, @sas_token_duration_key, 'SAS Token Duration (Day)', 'SAS Token Duration (Day)', @number_type, 1, 1, 3650,365,5);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [is_readonly], [order],[enable_copy])
VALUES (@schema_id, @connection_string_key, 'Connection String', 'Connection String', @text_type, 0, 1,6, 1);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [is_readonly], [order],[enable_copy])
VALUES (@schema_id, @sas_token_key, 'SAS Token', 'SAS Token', @text_type, 0, 1,7, 1);


-- Insert Schema Detail Option
SELECT @detail_id = [id] FROM [schema_details] WITH(NOLOCK) WHERE [key] = @tier_key AND [schema_id] = @schema_id;

INSERT INTO [schema_detail_options]([schema_detail_id], [id], [name], [order])
VALUES
    (@detail_id,'Basic', 'Basic', 1),
    (@detail_id,'Standard', 'Standard', 2),
    (@detail_id,'Premium', 'Premium', 3)
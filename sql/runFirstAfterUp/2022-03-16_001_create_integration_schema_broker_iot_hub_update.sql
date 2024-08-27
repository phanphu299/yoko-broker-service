DECLARE @select_type nvarchar(50) = 'select';
DECLARE @text_type nvarchar(50) = 'text';
DECLARE @number_type nvarchar(50) = 'number';
DECLARE @bool_type nvarchar(50) = 'bool';

DECLARE @schema_type nvarchar(50) = 'BROKER_IOT_HUB';
DECLARE @schema_name nvarchar(255) = 'Broker IoT Hub Schema';

DECLARE @tier_key nvarchar(50) = 'tier';
DECLARE @hub_unit_key nvarchar(50) = 'number_of_hub_units';
DECLARE @defender_key nvarchar(50) = 'defender_for_iot';
DECLARE @cloud_partition_key nvarchar(50) = 'device_to_cloud_partitions';
DECLARE @connection_string_key nvarchar(50) = 'connection_string';
DECLARE @hub_name_key nvarchar(50) = 'event_hub_name';
DECLARE @show_event_hub nvarchar(50) = 'show_event_hub';
DECLARE @enable_sharing nvarchar(50) = 'enable_sharing';

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
SELECT @schema_id = [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @schema_type;

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [default_value] , [order])
VALUES (@schema_id, @tier_key, 'Tier', 'Select Tier', @select_type, 1,'B1',1);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [min_value], [max_value], [default_value], [order])
VALUES (@schema_id, @hub_unit_key, 'Number of Hub Units', 'Number of Hub Units', @number_type, 1, 1, 20, 2,2);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [order],[default_value])
VALUES (@schema_id, @defender_key, 'Defender for IoT', 'Defender for IoT', @bool_type, 1,3, 'false');

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [min_value], [max_value], [default_value], [order])
VALUES (@schema_id, @cloud_partition_key, 'Device-to-cloud partitions', 'Device-to-cloud partitions', @number_type, 1, 1, 20, 4,4);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required] ,[default_value], [order])
VALUES (@schema_id, @enable_sharing, 'Sharing', 'Sharing', @bool_type, 1, 'false', 5);

-- Insert Schema Detail Option
SELECT @detail_id = [id] FROM [schema_details] WITH(NOLOCK) WHERE [key] = @tier_key AND [schema_id] = @schema_id;

INSERT INTO [schema_detail_options]([schema_detail_id], [id], [name], [order])
VALUES
    (@detail_id, 'B1', 'Basic 1',1),
    (@detail_id, 'B2', 'Basic 2',2),
    (@detail_id, 'B3', 'Basic 3',3),
    (@detail_id, 'S1', 'Standard 1',4),
    (@detail_id, 'S2', 'Standard 2',5),
    (@detail_id, 'S3', 'Standard 3',6)
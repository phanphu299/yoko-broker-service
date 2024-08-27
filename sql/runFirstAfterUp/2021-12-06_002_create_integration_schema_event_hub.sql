DECLARE @select_type nvarchar(50) = 'select';
DECLARE @text_type nvarchar(50) = 'text';
DECLARE @number_type nvarchar(50) = 'number';
DECLARE @bool_type nvarchar(50) = 'bool';

DECLARE @schema_type nvarchar(50) = 'INTEGRATION_EVENT_HUB';
DECLARE @schema_name nvarchar(255) = 'Integration Event Hub Schema';

DECLARE @connection_string_key nvarchar(50) = 'connection_string';
DECLARE @hub_name_key nvarchar(50) = 'event_hub_name';

DECLARE @hub_name_regex nvarchar(255) = '^[a-zA-Z]+[a-z0-9-_]*$';

DECLARE @schema_id uniqueidentifier;

-- Remove old
DELETE FROM [schema_detail_options] WHERE [schema_detail_id] IN (SELECT [id] FROM [schema_details] WITH(NOLOCK) WHERE [schema_id] IN (SELECT [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @schema_type));
DELETE FROM [schema_details] WHERE [schema_id] IN (SELECT [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @schema_type);
DELETE FROM [schemas] WHERE [type] = @schema_type;

-- Insert Schema
INSERT INTO [schemas]([name], [type])
VALUES (@schema_name, @schema_type);

-- Insert Schema Detail
SELECT @schema_id = [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @schema_type;

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [regex], [order], [enable_copy])
VALUES (@schema_id, @hub_name_key, 'Hub Name', 'Hub Name', @text_type, 1, @hub_name_regex, 1, 1);
INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [order], [enable_copy])
VALUES (@schema_id, @connection_string_key, 'Connection String', 'Connection String', @text_type, 1, 2, 1);

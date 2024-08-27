DECLARE @select_type nvarchar(50) = 'select';
DECLARE @text_type nvarchar(50) = 'text';
DECLARE @number_type nvarchar(50) = 'number';
DECLARE @bool_type nvarchar(50) = 'bool';

DECLARE @schema_type nvarchar(50) = 'BROKER_REST_API';
DECLARE @schema_name nvarchar(255) = 'Broker Rest api schema';

DECLARE @endpoint_key nvarchar(50) = 'endpoint';
DECLARE @api_key_key nvarchar(50) = 'api_key';

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

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [is_readonly], [order],[enable_copy])
VALUES (@schema_id, @endpoint_key, 'Endpoint', 'Endpoint', @text_type, 0, 1,1, 1);

-- INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [is_readonly], [order])
-- VALUES (@schema_id, @api_key_key, 'API Key', 'API Key', @text_type, 0, 1,2);
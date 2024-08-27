DECLARE @select_type nvarchar(50) = 'select';
DECLARE @text_type nvarchar(50) = 'text';
DECLARE @number_type nvarchar(50) = 'number';
DECLARE @bool_type nvarchar(50) = 'bool';

DECLARE @schema_type nvarchar(50) = 'WAYLAY_API';
DECLARE @schema_name nvarchar(255) = 'Rest api schema';

DECLARE @endpoint_key nvarchar(50) = 'endpoint';
DECLARE @api_key_key nvarchar(50) = 'api_key';
DECLARE @api_secret_key nvarchar(50) = 'api_secret';
DECLARE @interval_key nvarchar(50) = 'pooling_interval';
DECLARE @broker_endpoint_key nvarchar(50) = 'broker_endpoint';

--DECLARE @endpoint_regex nvarchar(255) = 'https://([a-z0-9-_:/\.]+)';

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

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [regex], [order], [default_value], [is_readonly])
VALUES (@schema_id, @endpoint_key, 'Endpoint', 'Endpoint', @text_type, 1, null, 1, N'https://dxpeng.msa2.apps.yokogawa.build', 1);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [order])
VALUES (@schema_id, @api_key_key, 'API Key', 'API Key', @text_type, 1,2);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [order])
VALUES (@schema_id, @api_secret_key, 'API Secret', 'API Secret', @text_type, 1,3);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [min_value], [max_value], [default_value], [order])
VALUES (@schema_id, @interval_key, 'Interval Time (in second)', 'Interval Time (in second)', @number_type, 1, 2, 86400, 80, 4);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [regex], [order], [default_value], [is_readonly])
VALUES (@schema_id, @broker_endpoint_key, 'Broker Endpoint', 'Broker Endpoint', @text_type, 1, null, 5, N'https://broker.msa2.apps.yokogawa.build', 1);
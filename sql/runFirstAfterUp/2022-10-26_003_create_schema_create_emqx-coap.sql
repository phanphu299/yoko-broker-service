DECLARE @select_type nvarchar(50) = 'select';
DECLARE @text_type nvarchar(50) = 'text';
DECLARE @number_type nvarchar(50) = 'number';
DECLARE @bool_type nvarchar(50) = 'bool';

DECLARE @schema_type nvarchar(50) = 'BROKER_EMQX_COAP';
DECLARE @schema_name nvarchar(255) = 'EMQX-COAP schema';

DECLARE @connection_mode nvarchar(50) = 'connection_mode';
DECLARE @uri_telemetry nvarchar(50) = 'uri_telemetry';
DECLARE @uri_command nvarchar(50) = 'uri_command';
DECLARE @password_length nvarchar(50) = 'password_length';

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

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [default_value], [data_type], [is_required], [is_readonly], [order],[enable_copy], [is_editable])
VALUES (@schema_id, @uri_telemetry, 'BROKER.SCHEMA.BROKER_EMQX_COAP.URI_TELEMETRY.NAME', 'BROKER.SCHEMA.BROKER_EMQX_COAP.URI_TELEMETRY.PLACE_HOLDER',null, @text_type, 0, 1,2, 1, 0),
       (@schema_id, @uri_command, 'BROKER.SCHEMA.BROKER_EMQX_COAP.URI_COMMAND.NAME', 'BROKER.SCHEMA.BROKER_EMQX_COAP.URI_COMMAND.PLACE_HOLDER', null, @text_type, 0, 1,3, 1, 0);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [default_value] , [order], [is_editable])
VALUES (@schema_id, @connection_mode, 'BROKER.SCHEMA.BROKER_EMQX_COAP.CONNECTION_MODE.NAME', 'BROKER.SCHEMA.BROKER_EMQX_COAP.CONNECTION_MODE.PLACE_HOLDER', @select_type, 1,'Connection',1, 0);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [min_value], [max_value], [default_value], [order])
VALUES (@schema_id, @password_length, 'BROKER.SCHEMA.BROKER_EMQX_COAP.PASSWORD_LENGTH.NAME', 'BROKER.SCHEMA.BROKER_EMQX_COAP.PASSWORD_LENGTH.PLACE_HOLDER', @number_type, 0, 10, 64, null, 4);

-- Insert Schema Detail Option
SELECT @detail_id = [id] FROM [schema_details] WITH(NOLOCK) WHERE [key] = @connection_mode AND [schema_id] = @schema_id;
INSERT INTO [schema_detail_options]([schema_detail_id], [code], [name], [order])
VALUES
    (@detail_id, 'Connection', 'Connection',1)

IF NOT EXISTS (SELECT id FROM schema_details WHERE [key] = 'host' AND name = 'BROKER.SCHEMA.BROKER_EMQX_COAP.HOST.NAME')
BEGIN 
	INSERT INTO schema_details (schema_id, [key], name, is_required, is_readonly, place_holder, data_type, created_utc, updated_utc, deleted, regex, min_value, max_value, default_value, depend_on_key, [order], enable_copy, is_editable, endpoint)
	SELECT schema_id, 'host' as [key], 'BROKER.SCHEMA.BROKER_EMQX_COAP.HOST.NAME' AS name, is_required, is_readonly, 
					'BROKER.SCHEMA.BROKER_EMQX_COAP.HOST.PLACE_HOLDER' as place_holder, data_type, created_utc, updated_utc, deleted, regex, min_value, max_value, default_value, depend_on_key, [order], enable_copy, is_editable, endpoint
	FROM schema_details WHERE [key] = 'uri_telemetry' AND name = 'BROKER.SCHEMA.BROKER_EMQX_COAP.URI_TELEMETRY.NAME'
END

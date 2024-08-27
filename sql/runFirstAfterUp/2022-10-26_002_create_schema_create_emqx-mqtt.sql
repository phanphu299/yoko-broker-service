DECLARE @select_type nvarchar(50) = 'select';
DECLARE @text_type nvarchar(50) = 'text';
DECLARE @number_type nvarchar(50) = 'number';
DECLARE @bool_type nvarchar(50) = 'bool';

DECLARE @schema_type nvarchar(50) = 'BROKER_EMQX_MQTT';
DECLARE @schema_name nvarchar(255) = 'EMQX-MQTT schema';

DECLARE @authentication_type nvarchar(50) = 'authentication_type';
DECLARE @host nvarchar(50) = 'host';
DECLARE @port nvarchar(50) = 'port';
DECLARE @telemetry_topic nvarchar(50) = 'telemetry_topic';
DECLARE @command_topic nvarchar(50) = 'command_topic';
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
VALUES (@schema_id, @telemetry_topic, 'BROKER.SCHEMA.BROKER_EMQX_MQTT.TELEMETRY_TOPIC.NAME', 'BROKER.SCHEMA.BROKER_EMQX_MQTT.TELEMETRY_TOPIC.PLACE_HOLDER', null, @text_type, 0, 1,2, 1, 0),
       (@schema_id, @command_topic, 'BROKER.SCHEMA.BROKER_EMQX_MQTT.COMMAND_TOPIC.NAME', 'BROKER.SCHEMA.BROKER_EMQX_MQTT.COMMAND_TOPIC.PLACE_HOLDER', null, @text_type, 0, 1,3, 1, 0),
       (@schema_id, @host, 'BROKER.SCHEMA.BROKER_EMQX_MQTT.HOST.NAME', 'BROKER.SCHEMA.BROKER_EMQX_MQTT.HOST.PLACE_HOLDER',null, @text_type, 0, 1,4, 1, 0),
       (@schema_id, @port, 'BROKER.SCHEMA.BROKER_EMQX_MQTT.PORT.NAME', 'BROKER.SCHEMA.BROKER_EMQX_MQTT.PORT.PLACE_HOLDER','1883', @text_type, 0, 1,5, 1, 0);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [default_value] , [order], [is_editable])
VALUES (@schema_id, @authentication_type, 'BROKER.SCHEMA.BROKER_EMQX_MQTT.AUTHENTICATION_TYPE.NAME', 'BROKER.SCHEMA.BROKER_EMQX_MQTT.AUTHENTICATION_TYPE.PLACE_HOLDER', @select_type, 1,'Simple',1, 0);

INSERT INTO [schema_details]([schema_id], [key], [name], [place_holder], [data_type], [is_required], [min_value], [max_value], [default_value], [order])
VALUES (@schema_id, @password_length, 'BROKER.SCHEMA.BROKER_EMQX_MQTT.PASSWORD_LENGTH.NAME', 'BROKER.SCHEMA.BROKER_EMQX_MQTT.PASSWORD_LENGTH.PLACE_HOLDER', @number_type, 0, 10, 64, null, 6);

-- Insert Schema Detail Option
SELECT @detail_id = [id] FROM [schema_details] WITH(NOLOCK) WHERE [key] = @authentication_type AND [schema_id] = @schema_id;
INSERT INTO [schema_detail_options]([schema_detail_id], [code], [name], [order])
VALUES
    (@detail_id, 'Simple', 'Simple',1);
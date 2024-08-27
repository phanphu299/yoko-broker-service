IF NOT EXISTS (SELECT id FROM schema_details WHERE [key] = 'host' AND name = 'BROKER.SCHEMA.BROKER_EMQX_COAP.HOST.NAME')
BEGIN 
	INSERT INTO schema_details (schema_id, [key], name, is_required, is_readonly, place_holder, data_type, created_utc, updated_utc, deleted, regex, min_value, max_value, default_value, depend_on_key, [order], enable_copy, is_editable, endpoint)
	SELECT schema_id, 'host' as [key], 'BROKER.SCHEMA.BROKER_EMQX_COAP.HOST.NAME' AS name, is_required, is_readonly, 
					'BROKER.SCHEMA.BROKER_EMQX_COAP.HOST.PLACE_HOLDER' as place_holder, data_type, created_utc, updated_utc, deleted, regex, min_value, max_value, default_value, depend_on_key, [order], enable_copy, is_editable, endpoint
	FROM schema_details WHERE [key] = 'uri_telemetry' AND name = 'BROKER.SCHEMA.BROKER_EMQX_COAP.URI_TELEMETRY.NAME'
END
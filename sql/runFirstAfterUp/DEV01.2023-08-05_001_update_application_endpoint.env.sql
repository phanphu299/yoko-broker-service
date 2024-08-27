DECLARE @schema_type nvarchar(50) = 'EVENT_TYPE_AHI_APPLICATION';
DECLARE @application_key nvarchar(50) = 'ahi_application';
DECLARE @schema_id uniqueidentifier;
DECLARE @host nvarchar(1024) = 'https://ahs-dev01-ppm-be-sea-wa.azurewebsites.net';
SELECT @schema_id = [id] FROM [schemas] WITH(NOLOCK) WHERE [type] = @schema_type;
update schema_details
set endpoint = concat(@host, '/acm/applications')
where schema_id = @schema_id and [key] = @application_key
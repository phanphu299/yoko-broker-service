CREATE OR ALTER view v_integration_eventhub_runnable
AS
SELECT 
      i.id AS id
    , i.name AS name
    , id.content as Content
FROM 
    integrations i with(nolock)
	inner join integration_details id with(nolock) on i.id = id.Integration_id
WHERE 
    i.type ='INTEGRATION_EVENT_HUB' and i.is_processing = 0 and id.content is not null and id.content != '{}'
GO
CREATE OR ALTER view v_broker_eventhub_runnable
AS
SELECT 
    b.id AS id
    , b.name AS name
    , bd.content as Content
FROM 
    brokers b with(nolock)
	inner join broker_details bd with(nolock) on b.id = bd.broker_id
WHERE 
    b.type ='BROKER_EVENT_HUB' and b.is_processing = 0 and bd.content is not null and bd.content != '{}'
GO
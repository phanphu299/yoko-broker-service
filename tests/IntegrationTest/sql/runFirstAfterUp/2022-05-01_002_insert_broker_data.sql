insert into brokers (id, name, status, type)
values ('43b41948-0f97-4d0a-8905-265f0016e5ff','YDX Broker', 'AC', 'BROKER_EVENT_HUB');

insert into broker_details (broker_id, content)
values ('43b41948-0f97-4d0a-8905-265f0016e5ff', '{"event_hub_name":"ingestion-local","throughput_units":1,"auto_inflate":false,"max_throughput_units":20,"tier":"Basic","connection_string":"Endpoint=sb://ydxbiz.servicebus.windows.net/;SharedAccessKeyName=Root;SharedAccessKey=Mo4fBuSY6HMrTWHUUxp11/xEgwkNMg+g9iizxu+NMlU=;EntityPath=ingestion-local","event_hub_id":"/subscriptions/baebc905-5667-42eb-b183-b86c10a758cf/resourceGroups/YDXBiz/providers/Microsoft.EventHub/namespaces/ydxbiz"}');
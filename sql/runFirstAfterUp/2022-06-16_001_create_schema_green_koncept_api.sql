
--######################### schema_details  on broker table
--delete from schemas where type = 'GREEN_KONCEPT_API'
declare @LookupKey varchar(50) = 'GREEN_KONCEPT_API';

if(not exists(select Id from [schemas] where type =  @LookupKey ))
begin
	insert into [schemas](type, name, created_utc, updated_utc)
	values (@LookupKey, 'Green Koncepts Integration', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
end

declare @SchemaId varchar(50);
select @SchemaId = id from [schemas] where type = @LookupKey;

delete from schema_details where schema_id = @SchemaId;
insert into schema_details (schema_id, [key], name, is_required, place_holder, data_type, created_utc, updated_utc, deleted, regex, min_value, max_value, [order])
values 
(@SchemaId,'endpoint', 'Endpoint',1 , 'Endpoint', 'text', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 0, 'https://([a-z0-9-_:/\.]+)', null, null,1),
(@SchemaId,'client_id', 'clientId',1 , 'clientId', 'text', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 0, null, null, null,2),
(@SchemaId,'client_secret', 'clientSecret',1 , 'clientSecret', 'text', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 0, null, null, null,3);



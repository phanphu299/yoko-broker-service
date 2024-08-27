update broker_details 
set content = replace(content, '"tierName":"Basic 1"','"tierName":"Standard 1"')
where content like '%"tier":"S1"%';

update broker_details 
set content = replace(content, '"tierName":"Basic"','"tierName":"Standard"')
where content like '%"tier":"Standard"%';
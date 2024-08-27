create table integrations(
id uniqueidentifier default newsequentialid() primary key,
name nvarchar(255) not null, 
status varchar(2) not null default 'AC', 
type varchar(50) not null,
created_utc datetime2 not null default getutcdate(),
updated_utc datetime2 not null default getutcdate(),
deleted bit not null default 0
);

create table integration_details(
id uniqueidentifier default newsequentialid() primary key,
integration_id uniqueidentifier not null,
content nvarchar(max) not null,
created_utc datetime2 not null default getutcdate(),
updated_utc datetime2 not null default getutcdate(),
deleted bit not null default 0,
CONSTRAINT fk_integration_details_activity_id       FOREIGN KEY(integration_id)  	  REFERENCES integrations(id)
);
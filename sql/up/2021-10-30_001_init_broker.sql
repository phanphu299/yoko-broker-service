create table brokers(
id uniqueidentifier default newsequentialid() primary key,
name nvarchar(255) not null, 
status varchar(2) not null default 'AC', 
created_utc datetime2 not null default getutcdate(),
updated_utc datetime2 not null default getutcdate(),
deleted bit not null default 0
);

create table broker_details(
id uniqueidentifier default newsequentialid() primary key,
broker_id uniqueidentifier not null,
content nvarchar(max) not null,
created_utc datetime2 not null default getutcdate(),
updated_utc datetime2 not null default getutcdate(),
deleted bit not null default 0,
CONSTRAINT fk_broker_details_activity_id       FOREIGN KEY(broker_id)  	  REFERENCES brokers(id)
);
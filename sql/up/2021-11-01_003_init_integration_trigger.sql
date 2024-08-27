create table integration_schedulers(
id uniqueidentifier default newsequentialid() primary key,
integration_id uniqueidentifier not null,
device_id nvarchar(255) not null,
pooling_interval int not null default 2,
created_utc datetime2 not null default getutcdate(),
updated_utc datetime2 not null default getutcdate(),
deleted bit not null default 0,
CONSTRAINT fk_integration_schedulers_integration_id       FOREIGN KEY(integration_id)  	  REFERENCES integrations(id)
);

create table scheduler_histories(
id uniqueidentifier default newsequentialid() primary key,
scheduler_id uniqueidentifier not null,
last_run datetime2 not null default getutcdate(),
last_run_result nvarchar(255) default 'success',
created_utc datetime2 not null default getutcdate(),
updated_utc datetime2 not null default getutcdate(),
deleted bit not null default 0,
CONSTRAINT fk_scheduler_histories_scheduler_id       FOREIGN KEY(scheduler_id)  	  REFERENCES integration_schedulers(id)
);
create table schemas(
id uniqueidentifier default newsequentialid() primary key,
type nvarchar(50) not null, 
name nvarchar(255) not null,
created_utc datetime2 not null default getutcdate(),
updated_utc datetime2 not null default getutcdate(),
deleted bit not null default 0
);

create table schema_details(
id uniqueidentifier default newsequentialid() primary key,
schema_id uniqueidentifier not null,
[key] nvarchar(50) not null,
name nvarchar(255) not null, 
is_required bit not null default 0,
is_readonly bit not null default 0,
place_holder nvarchar(2048) null,
data_type nvarchar(50) not null,
created_utc datetime2 not null default getutcdate(),
updated_utc datetime2 not null default getutcdate(),
deleted bit not null default 0,
CONSTRAINT fk_schema_details_schema_id       FOREIGN KEY(schema_id)  	  REFERENCES schemas(id)
);

create table schema_detail_options(
    id nvarchar(255) not null primary key,
    name nvarchar(255) not null,
    schema_detail_id uniqueidentifier not null,
    created_utc datetime2 not null default getutcdate(),
    updated_utc datetime2 not null default getutcdate(),
    deleted bit not null default 0
)
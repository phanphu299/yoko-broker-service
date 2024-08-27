CREATE TABLE entity_tags (
    id bigint IDENTITY(1,1) PRIMARY KEY,
    tag_id bigint NOT NULL,
    entity_id_varchar varchar(100) NULL,
    entity_id_int int NULL,
    entity_id_long bigint NULL,
    entity_id_uuid UNIQUEIDENTIFIER NULL,
    entity_type varchar(100) NOT NULL
);
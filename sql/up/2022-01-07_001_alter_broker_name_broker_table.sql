IF EXISTS (SELECT * 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_NAME = 'unique_broker_name')
BEGIN
    alter table brokers
    DROP CONSTRAINT unique_broker_name
    alter table brokers
    ADD CONSTRAINT unique_broker_name UNIQUE (name, deleted);
END 
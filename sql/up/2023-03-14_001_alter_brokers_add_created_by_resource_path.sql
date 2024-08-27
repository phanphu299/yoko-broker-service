-- Brokers
IF NOT EXISTS(SELECT 1 FROM sys.columns  WHERE name = 'resource_path' AND object_id = object_id('brokers'))
BEGIN
    ALTER TABLE brokers ADD resource_path varchar(1024);
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'created_by' AND object_id = object_id('brokers'))
BEGIN
    ALTER TABLE brokers ADD created_by varchar(50) default 'thanh.tran@yokogawa.com';
END

IF NOT EXISTS(SELECT 1 FROM SYS.INDEXES 
          WHERE NAME = 'idx_brokers_resourcepath_createdby'
          AND OBJECT_ID = OBJECT_ID('brokers'))
BEGIN
    CREATE INDEX idx_brokers_resourcepath_createdby ON brokers(resource_path, created_by);
END
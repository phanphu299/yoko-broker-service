DECLARE @TableName varchar(100) = 'brokers'
DECLARE @ConstraintName NVARCHAR(500);
SELECT  @ConstraintName = obj_Constraint.NAME
FROM   sys.objects obj_table 
    JOIN sys.objects obj_Constraint 
        ON obj_table.object_id = obj_Constraint.parent_object_id 
    JOIN sys.sysconstraints constraints 
         ON constraints.constid = obj_Constraint.object_id 
    JOIN sys.columns columns 
         ON columns.object_id = obj_table.object_id 
        AND columns.column_id = constraints.colid 
WHERE obj_Constraint.type = 'D' 
AND obj_table.NAME=@TableName and columns.NAME = 'created_by';
IF (@ConstraintName IS NOT NULL)
BEGIN
	EXEC ('ALTER TABLE [' + @TableName + '] drop constraint [' + @ConstraintName +']')
END

DROP INDEX idx_brokers_resourcepath_createdby ON brokers;
ALTER TABLE brokers ALTER COLUMN created_by NVARCHAR(255);
ALTER TABLE brokers ADD CONSTRAINT df_brokers_created_by DEFAULT 'thanh.tran@yokogawa.com' FOR [created_by]; 
CREATE INDEX idx_brokers_resourcepath_createdby ON brokers(resource_path, created_by);
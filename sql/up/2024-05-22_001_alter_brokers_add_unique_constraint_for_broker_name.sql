-- Update duplicated broker name before add unique constraint
-- Bug: https://dev.azure.com/assethealthinsights/Asset%20Backlogs/_workitems/edit/98293/

;WITH cte AS
(
  SELECT
      ROW_NUMBER() OVER(PARTITION BY name  ORDER BY name ) AS ROWNUMBER,
      name 
  FROM brokers
  WHERE deleted = 0
)

UPDATE cte SET name = CONCAT(name, ' ', (ROWNUMBER - 1))
WHERE ROWNUMBER > 1

-- drop existing constraint/index
IF (OBJECT_ID('dbo.UC_brokers_name', 'UQ') IS NOT NULL)
BEGIN
    ALTER TABLE brokers DROP CONSTRAINT UC_brokers_name
END

DROP INDEX IF EXISTS UC_brokers_name
ON brokers;

-- add new index
CREATE UNIQUE INDEX [UC_brokers_name] 
ON brokers (name)
WHERE deleted = 0;
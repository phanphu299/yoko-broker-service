DECLARE @schema_id uniqueidentifier;
DECLARE @name_key nvarchar(255);
DECLARE @place_holder_key nvarchar(2048);
DECLARE @key nvarchar(50);
DECLARE @type nvarchar(50);
DECLARE @getdetail CURSOR

SET @getdetail = CURSOR FOR
SELECT b.type,
       a.[key]
FROM   [schema_details] a
JOIN [schemas] b on b.id = a.schema_id

OPEN @getdetail
FETCH NEXT
FROM @getdetail INTO @type, @key
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @schema_id = id FROM [schemas] WHERE type = @type;
    SET @name_key = 'BROKER.SCHEMA.'+ @type +'.' + UPPER(@key) + '.NAME';
    SET @place_holder_key = 'BROKER.SCHEMA.'+ @type +'.' + UPPER(@key) + '.PLACE_HOLDER';
    UPDATE schema_details SET name = @name_key, place_holder = @place_holder_key WHERE [key] = @key AND schema_id = @schema_id ;
    FETCH NEXT
    FROM @getdetail INTO @type, @key
END

CLOSE @getdetail
DEALLOCATE @getdetail


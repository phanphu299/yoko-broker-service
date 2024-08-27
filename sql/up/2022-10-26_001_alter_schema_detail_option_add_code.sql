ALTER TABLE schema_detail_options
ADD CONSTRAINT df_schema_detail_options_id DEFAULT NEWID() FOR id;

ALTER TABLE schema_detail_options ADD [code] NVARCHAR(255);

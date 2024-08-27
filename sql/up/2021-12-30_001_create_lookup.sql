
CREATE TABLE [lookups] (
  [code] VARCHAR(255) NOT NULL,
  [name] NVARCHAR(255) NOT NULL,
  [lookup_type_code] VARCHAR(255) NOT NULL,
  [active] BIT NOT NULL DEFAULT 1,
  CONSTRAINT pk_lookups PRIMARY KEY([code], [lookup_type_code]),
)
GO

CREATE TABLE [emqx_topics] (
  [id] UNIQUEIDENTIFIER DEFAULT newsequentialid() primary key,
  [broker_id] UNIQUEIDENTIFIER NOT NULL,
  [client_id] UNIQUEIDENTIFIER NOT NULL,
  [access_token] NVARCHAR(MAX) NOT NULL,
  [topic_name] NVARCHAR(255) NOT NULL,
  [created_utc] DATETIME2 NOT NULL DEFAULT getutcdate(),
  [updated_utc] DATETIME2 NOT NULL DEFAULT getutcdate(),
  [deleted] BIT NOT NULL DEFAULT 0,
  CONSTRAINT fk_emqx_topics_activity_id FOREIGN KEY(broker_id) REFERENCES brokers(id)
)
GO
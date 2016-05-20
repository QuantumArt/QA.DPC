IF NOT EXISTS (SELECT * FROM sys.[columns] WHERE [object_id]=OBJECT_ID('Messages') AND NAME='UserID')
 ALTER TABLE [Messages] ADD UserID int NOT NULL CONSTRAINT DF_Messages_UserID DEFAULT 1

 IF NOT EXISTS (SELECT * FROM sys.[columns] WHERE [object_id]=OBJECT_ID('Messages') AND NAME='UserName')
  ALTER TABLE [Messages] ADD UserName varchar(100) NOT NULL CONSTRAINT DF_Messages_UserName DEFAULT 'Admin'


IF NOT EXISTS (SELECT * FROM sys.[columns] WHERE [object_id]=OBJECT_ID('Messages') AND NAME='DataKey')
ALTER TABLE [Messages] ADD DataKey int NOT NULL CONSTRAINT DF_Messages_DataKey DEFAULT 0

IF NOT EXISTS (SELECT * FROM sys.[columns] WHERE NAME='BinData' AND [object_id]=OBJECT_ID('Tasks'))
	ALTER TABLE dbo.Tasks ADD BinData varbinary(max) NULL

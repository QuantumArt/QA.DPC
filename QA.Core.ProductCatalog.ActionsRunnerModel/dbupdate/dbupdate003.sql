IF NOT EXISTS (SELECT * FROM sys.[columns] WHERE NAME='ExclusiveCategory' AND [object_id]=OBJECT_ID('Tasks'))
	ALTER TABLE dbo.Tasks ADD ExclusiveCategory varchar(50) NULL
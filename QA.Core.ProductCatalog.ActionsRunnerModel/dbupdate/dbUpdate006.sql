IF EXISTS(SELECT * FROM sys.[columns] WHERE [object_id]=OBJECT_ID('TaskStates') AND name='Name')
BEGIN
	ALTER TABLE [dbo].[TaskStates] ALTER COLUMN [Name] NVARCHAR(50) NOT NULL
END
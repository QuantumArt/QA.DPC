﻿IF EXISTS(SELECT * FROM sys.[columns] WHERE [object_id]=OBJECT_ID('Tasks') AND name='message')
BEGIN
	ALTER TABLE Tasks ALTER COLUMN [MESSAGE] NVARCHAR(MAX) null
END
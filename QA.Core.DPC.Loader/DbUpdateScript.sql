﻿IF NOT EXISTS(SELECT * FROM sys.types WHERE NAME='Ids')
BEGIN
	CREATE TYPE [dbo].Ids AS TABLE(
		[Id] [int] NOT NULL,
		PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
	)
END
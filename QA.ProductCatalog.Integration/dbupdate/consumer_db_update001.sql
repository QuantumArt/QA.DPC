SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ProductRelevance') is NULL
BEGIN
	CREATE TABLE [dbo].[ProductRelevance](
		[ProductID] [int] NOT NULL,
		[LastUpdateTime] [datetime] NOT NULL,
		[StatusID] [tinyint] NOT NULL,
		[IsLive] [bit] NOT NULL,
	 CONSTRAINT [PK_ProductRelevance] PRIMARY KEY NONCLUSTERED 
	(
		[ProductID] ASC,
		[IsLive] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


	ALTER TABLE [dbo].[ProductRelevance] ADD  CONSTRAINT [DF_ProductRelevance_LastUpdateTime]  DEFAULT (getdate()) FOR [LastUpdateTime]

	ALTER TABLE [dbo].[ProductRelevance] ADD  DEFAULT ((1)) FOR [IsLive]
END
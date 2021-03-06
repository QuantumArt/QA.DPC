CREATE TABLE [dbo].[Messages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Channel] [nvarchar](50) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Data] [nvarchar](max) NOT NULL,
	[Method] [nvarchar](10) NOT NULL,
	[UserID] [int] NOT NULL,
	[UserName] [varchar](100) NOT NULL,
	[DataKey] [int] NOT NULL,
 CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

GO
ALTER TABLE [dbo].[Messages] ADD  CONSTRAINT [DF_Messages_UserID]  DEFAULT ((1)) FOR [UserID]
GO
ALTER TABLE [dbo].[Messages] ADD  CONSTRAINT [DF_Messages_UserName]  DEFAULT ('Admin') FOR [UserName]
GO
ALTER TABLE [dbo].[Messages] ADD  CONSTRAINT [DF_Messages_DataKey]  DEFAULT ((0)) FOR [DataKey]
GO

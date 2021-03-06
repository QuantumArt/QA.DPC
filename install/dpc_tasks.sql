CREATE TABLE [dbo].[Schedules](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Enabled] [bit] NOT NULL,
	[CronExpression] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Schedules] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Tasks](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CreatedTime] [datetime] NOT NULL,
	[LastStatusChangeTime] [datetime] NULL,
	[Name] [varchar](500) NOT NULL,
	[StateID] [int] NOT NULL,
	[Data] [nvarchar](max) NULL,
	[UserID] [int] NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[Progress] [tinyint] NULL,
	[Message] [nvarchar](max) NULL,
	[IsCancellationRequested] [bit] NOT NULL,
	[IsCancelled] [bit] NOT NULL,
	[DisplayName] [nvarchar](4000) NOT NULL,
	[ScheduleID] [int] NULL,
	[ScheduledFromTaskID] [int] NULL,
	[ExclusiveCategory] [varchar](50) NULL,
	[Config] [nvarchar](max) NULL,
	[BinData] [varbinary](max) NULL,
 CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[TaskStates](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_TaskStates] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


ALTER TABLE [dbo].[Schedules] ADD  CONSTRAINT [DF_TaskSchedules_Enabled]  DEFAULT ((1)) FOR [Enabled]
GO

ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO

ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_IsCancellationRequested]  DEFAULT ((0)) FOR [IsCancellationRequested]
GO

ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_IsCancelled]  DEFAULT ((0)) FOR [IsCancelled]
GO

ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_Schedules] FOREIGN KEY([ScheduleID])
REFERENCES [dbo].[Schedules] ([ID])
GO

ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_TaskStates] FOREIGN KEY([StateID])
REFERENCES [dbo].[TaskStates] ([ID])
GO

ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_TasksScheduledFrom_Tasks] FOREIGN KEY([ScheduledFromTaskID])
REFERENCES [dbo].[Tasks] ([ID])
GO

SET IDENTITY_INSERT [dbo].[TaskStates] ON 

INSERT [dbo].[TaskStates] ([ID], [Name]) VALUES (1, N'Новая')
INSERT [dbo].[TaskStates] ([ID], [Name]) VALUES (2, N'Выполняется')
INSERT [dbo].[TaskStates] ([ID], [Name]) VALUES (3, N'Завершена')
INSERT [dbo].[TaskStates] ([ID], [Name]) VALUES (4, N'Ошибка')
INSERT [dbo].[TaskStates] ([ID], [Name]) VALUES (5, N'Отменена')
SET IDENTITY_INSERT [dbo].[TaskStates] OFF
GO

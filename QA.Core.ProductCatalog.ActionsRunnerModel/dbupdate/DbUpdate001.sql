IF OBJECT_ID('Schedules') IS NULL
BEGIN
	CREATE TABLE [dbo].[Schedules](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[Enabled] [bit] NOT NULL,
		[CronExpression] [varchar](100) NOT NULL,
	 CONSTRAINT [PK_Schedules] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


	ALTER TABLE [dbo].[Schedules] ADD  CONSTRAINT [DF_TaskSchedules_Enabled]  DEFAULT ((1)) FOR [Enabled]
END


IF NOT EXISTS(SELECT * FROM sys.[columns] WHERE [object_id]=OBJECT_ID('Tasks') AND NAME='ScheduleID')
BEGIN
	
	ALTER TABLE Tasks ADD [ScheduleID] INT NULL
	
	ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_Schedules] FOREIGN KEY([ScheduleID])
	REFERENCES [dbo].[Schedules] ([ID])

	ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_Tasks_Schedules]
END

IF NOT EXISTS(SELECT * FROM sys.[columns] WHERE [object_id]=OBJECT_ID('Tasks') AND NAME='ScheduledFromTaskID')
BEGIN
	
	ALTER TABLE Tasks ADD ScheduledFromTaskID INT NULL
	
	ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_TasksScheduledFrom_Tasks] FOREIGN KEY([ScheduledFromTaskID])
	REFERENCES [dbo].[Tasks] ([ID])

	ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_TasksScheduledFrom_Tasks]
END
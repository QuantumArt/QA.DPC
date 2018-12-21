declare @userId int = 1
declare @userName nvarchar(100) = 'Admin'
declare @name nvarchar(max) = 'ValidationAction'
declare @displayName nvarchar(4000) = 'Валидация'
declare @message nvarchar(max) = 'Расписание валидации'
declare @updateChunkSize int = 1000
declare @maxDegreeOfParallelism int = 4
declare @schedule nvarchar(100) = '0 8 * * *'
declare @customerCode nvarchar(100) = 'dpc_catalog'

declare @taskContext nvarchar(max) =
'{
  "ActionContext": {
    "UserName": "' + @userName +'",
    "UserId": ' + ltrim(str(@userId)) + ',
    "BackendSid": "00000000-0000-0000-0000-000000000000",
    "ContentId": 0,
    "ContentItemIds": [],
    "ActionCode": "custom_635712541162304290",
    "CustomerCode": "' + @customerCode +'",
    "Parameters": { "UpdateChunkSize": "' + ltrim(str(@updateChunkSize)) + '", "MaxDegreeOfParallelism": "' + ltrim(str(@maxDegreeOfParallelism)) + '" }
  },
  "Description": null,
  "IconUrl": "http://static.mts.ru/dpc_upload/icons/check.png"
}'


declare @scheduleId int
select @scheduleId = ScheduleId FROM Tasks where [Name] = @name and ScheduleId is not null

if (@scheduleId is null)
begin
	declare @ids table ( id int )
	insert Schedules([Enabled], CronExpression)	
	output inserted.ID into @ids
	values (1, @schedule)
	select @scheduleId=id from @ids
end
else
begin
	update Schedules set CronExpression = @schedule where id = @scheduleId
end

if exists(select null from Tasks where [Name] = @name and ScheduleId = @scheduleId)
	update Tasks
	set
		[Data]=@taskContext,
		[UserID] = @userId,
		[UserName] = @userName,
		[Message] = @message,
		[DisplayName] = @displayName
	where
		[Name] = @name and
		ScheduleId = @scheduleId
else
	insert Tasks(
		[CreatedTime],
		[LastStatusChangeTime],
		[Name],
		[StateID],
		[Data],
		[UserID],
		[UserName],
		[Progress],
		[Message],
		[IsCancellationRequested],
		[IsCancelled],
		[DisplayName],
		[ScheduleID])
	values(
		getdate(),
		getdate(),
		@name,
		3,
		@taskContext,
		@userId,
		@userName,
		100,
		@message,
		0,
		0,
		@displayName,
		@scheduleId)
	







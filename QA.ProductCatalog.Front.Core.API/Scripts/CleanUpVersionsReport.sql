declare @interval int = 0
declare @endInterval int = 150
declare @stepInterval int = 10
declare @total int
declare @todelete int
declare @rest int
declare @date datetime
declare @report table
(
  interval int, 
  total int,
  todelete int,
  rest int
)

select @total = count(*)
from ProductVersions with (nolock)

while @interval <= @endInterval
begin

	set @date = dateadd(day, -@interval, getdate())

	select @todelete = count(*)
	from ProductVersions with (nolock)
	where [Modification] < @date

	select @rest = count(*)
	from (
		select [DpcId] from ProductVersions v with (nolock)
		where [Modification] < @date
		group by [DpcId], [IsLive], [Language], [Format]
	) v

	insert @report
	values(@interval, @total, @todelete, @rest)
	set @interval = @interval + @stepInterval
end

select
	interval [Interval],
	total [Versions],
	todelete - rest [Deleted versions],
	100.0 * (todelete - rest) / total [Ratio, %]
from @report
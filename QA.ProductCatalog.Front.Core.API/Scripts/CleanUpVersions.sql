declare @interval int = 30
declare @date datetime = dateadd(day, -@interval, getdate())
declare @count int = 1

while @count > 0
begin
	with t as (
	select top(250) v.[Id] from
		ProductVersions v with (nolock)
	where
		exists (
			select null
			from ProductVersions as v2 with (nolock)
			where
				v2.[Id] > v.[Id]
				and v2.[DpcId] = v.[DpcId]
				and v2.[IsLive] = v.[IsLive]
				and v2.[Language] = v.[Language]
				and v2.[Format] = v.[Format]
				and v2.[Modification] < @date)
		and v.[Modification] < @date
	order by v.[Id])
	delete from t 
	select @count = @@ROWCOUNT

	print @count
end


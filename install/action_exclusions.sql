

declare @custom_id int
select @custom_id = id from backend_action where name = 'Удалить' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_remove_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_remove_article'))

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('remove_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('remove_article'))

select @custom_id = id from backend_action where name = 'Удалить маркетинговый продукт' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_remove_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_remove_article'))

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('remove_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('remove_article'))


select @custom_id = id from backend_action where name = 'Удалить из архива' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_remove_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_remove_archive_article'))

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('remove_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('remove_archive_article'))


select @custom_id = id from backend_action where name = 'Удалить маркетинговый продукт из архива' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_remove_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_remove_archive_article'))

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('remove_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('remove_archive_article'))


select @custom_id = id from backend_action where name = 'Архивировать' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_move_to_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_move_to_archive_article'))

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('move_to_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('move_to_archive_article'))


select @custom_id = id from backend_action where name = 'Архивировать Маркетинговый продукт' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_move_to_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_move_to_archive_article'))

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('move_to_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('move_to_archive_article'))


select @custom_id = id from backend_action where name = 'Восстановить' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_restore_from_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_restore_from_archive_article'))

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('restore_from_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('restore_from_archive_article'))


select @custom_id = id from backend_action where name = 'Восстановить Маркетинговый продукт' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_restore_from_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_restore_from_archive_article'))

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('restore_from_archive_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('restore_from_archive_article'))



select @custom_id = id from backend_action where name = 'Публиковать' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_publish_articles')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_publish_articles'))

select @custom_id = id from backend_action where name = 'Публиковать Маркетинговый продукт' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('multiple_publish_articles')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('multiple_publish_articles'))

select @custom_id = id from backend_action where name = 'Клонировать' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('copy_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('copy_article'))

select @custom_id = id from backend_action where name = 'Клонировать Маркетинговый продукт' and code like 'custom%'

if not exists(select * from action_exclusions where EXCLUDED_BY_ID = @custom_id and EXCLUDES_ID = dbo.qp_action_id('copy_article')) 
	insert into action_exclusions values (@custom_id, dbo.qp_action_id('copy_article'))







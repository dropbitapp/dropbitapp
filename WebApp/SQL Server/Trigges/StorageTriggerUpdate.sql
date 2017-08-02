/*
	In this trigger, we are assuming the following:
	before StorageToRecord table is updated, we always know what RecordId is.
*/
go
if object_id('storage_history_update', 'TR') is not null
drop trigger storage_history_update
go

create trigger storage_history_update 
   on  dbo.StorageToRecord_v3
   after insert,update
as 
begin
	declare @RecordId int
	declare @Storage nvarchar(50)
	declare @StorageID int
	declare @Identifier nvarchar(5)
	declare @F2HID int

    -- Update statements for trigger here
	if(update(RecordId))

		select @RecordId = inserted.RecordId from inserted

		select @Identifier = inserted.TableIdentifier from inserted

		select @StorageID = inserted.StorageID from inserted

		set @Storage = (select Name from dbo.Storage_v3 where StorageID = @StorageID)
	
		set @F2HID = (select F2HID from dbo.F2H where FName = 'Storage')

		if(@Identifier = 'pur')
			insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
				values (
				@RecordId,
				@F2HID,
				@Storage,
				getdate()
				)
		if(@Identifier = 'prod')
			insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
				values (
				@RecordId,
				@F2HID,
				@Storage,
				getdate()
				)
		
end
go
-- ==============================================
-- Create dml trigger template Azure SQL Database 
-- ==============================================
-- Drop the dml trigger if it already exists
go
if object_id('spirit_history_update', 'TR') is not null
drop trigger spirit_history_update
go

create trigger spirit_history_update 
   on  dbo.ProductionToSpirit_v3
   after insert, update
as 
begin


	declare @ProductionID int
	declare @Spirit nvarchar(50)
	declare @SpiritID int
	declare @F2HID int

    -- Update statements for trigger here
	if(update(SpiritID))

		select @SpiritID =  cast(inserted.SpiritID as nvarchar(50)) from inserted
		
		select @ProductionID =  cast(inserted.ProductionID as nvarchar(50)) from inserted
	
		set @Spirit = (select Name from dbo.Spirit_v3 where SpiritID = @SpiritID)
	
		set @F2HID = (select F2HID from dbo.F2H where FName = 'SpiritType')
			
		insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
		values (
		@ProductionID,
		@F2HID,
		@Spirit,
		getdate()
		)
end
go
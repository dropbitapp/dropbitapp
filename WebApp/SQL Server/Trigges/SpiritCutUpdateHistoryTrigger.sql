-- ==============================================
-- Create dml trigger template Azure SQL Database 
-- ==============================================
-- Drop the dml trigger if it already exists
go
if object_id('spiritCut_history_update', 'TR') is not null
drop trigger spiritCut_history_update
go

create trigger spiritCut_history_update 
   on  dbo.ProductionToSpiritCut_v3
   after insert, update
as 
begin


	declare @ProductionID int
	declare @SpiritCut nvarchar(50)
	declare @SpiritCutID int
	declare @F2HID int

    -- Update statements for trigger here
	if(update(SpiritCutID))

		select @SpiritCutID =  cast(inserted.SpiritCutID as nvarchar(50)) from inserted
		
		select @ProductionID =  cast(inserted.ProductionID as nvarchar(50)) from inserted
	
		set @SpiritCut = (select Name from dbo.SpiritCut_v3 where SpiritCutID = @SpiritCutID)
	
		set @F2HID = (select F2HID from dbo.F2H where FName = 'SpiritCut')
			
		insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
		values (
		@ProductionID,
		@F2HID,
		@SpiritCut,
		getdate()
		)
end
go
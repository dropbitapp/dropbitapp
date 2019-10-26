-- ==============================================
-- Create dml trigger template Azure SQL Database 
-- ==============================================
-- Drop the dml trigger if it already exists
go
if object_id('vbw_history_update', 'TR') is not null
drop trigger vbw_history_update
go

create trigger vbw_history_update 
   on  dbo.VolumeByWeightLB_v3
   after update
as 
begin

	declare @PurchaseID int
	declare @ProductionID int
	declare @VolumeByWeightLB nvarchar(50)
	declare @VolumeByWeightLBID int
	declare @F2HID int

    -- Update statements for trigger here
	if(update(Value))

		select @VolumeByWeightLB =  cast(inserted.Value as nvarchar(50)) from inserted
	
		select @VolumeByWeightLBID = inserted.VolumeByWeightLBID from inserted
	
		set @F2HID = (select F2HID from dbo.F2H where FName = 'VolumeByWeight')
		
		if exists(select * from Purchase_v3 where @VolumeByWeightLBID = VolumeByWeightLBID)
			set @PurchaseID = (select PurchaseID from Purchase_v3 where @VolumeByWeightLBID = VolumeByWeightLBID)
			if(@PurchaseID > 0)	
				insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
				values (
				@PurchaseID,
				@F2HID,
				@VolumeByWeightLB,
				getdate()
				)
		if exists(select * from Production_v3 where @VolumeByWeightLBID = VolumeByWeightLBID)
			set @ProductionID = (select ProductionID from Production_v3 where @VolumeByWeightLBID = VolumeByWeightLBID)
			if(@ProductionID > 0)	
				insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
				values (
				@ProductionID,
				@F2HID,
				@VolumeByWeightLB,
				getdate()
				)
end
go
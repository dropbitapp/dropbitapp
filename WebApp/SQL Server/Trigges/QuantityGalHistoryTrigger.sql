-- ==============================================
-- Create dml trigger template Azure SQL Database 
-- ==============================================
-- Drop the dml trigger if it already exists
go
if object_id('quantityGallon_history_update', 'TR') is not null
drop trigger quantityGallon_history_update
go

create trigger quantityGallon_history_update 
   on  dbo.QuantityGal_v3
   after update
as 
begin

	declare @PurchaseID int
	declare @ProductionID int
	declare @QuantityGal nvarchar(50)
	declare @QuantityGalID int
	declare @F2HID int

    -- Update statements for trigger here
	if(update(Value))

		select @QuantityGal =  cast(inserted.Value as nvarchar(50)) from inserted
	
		select @QuantityGalID = inserted.QuantityGalID from inserted
	
		set @F2HID = (select F2HID from dbo.F2H where FName = 'QuantityGal')
		
		if exists(select * from Purchase_v3 where @QuantityGalID = QuantityGalID)
			set @PurchaseID = (select PurchaseID from Purchase_v3 where @QuantityGalID = QuantityGalID)
			if(@PurchaseID > 0)	
				insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
				values (
				@PurchaseID,
				@F2HID,
				@QuantityGal,
				getdate()
				)
		if exists(select * from Production_v3 where @QuantityGalID = QuantityGalID)
			set @ProductionID = (select ProductionID from Production_v3 where @QuantityGalID = QuantityGalID)
			if(@ProductionID > 0)	
				insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
				values (
				@ProductionID,
				@F2HID,
				@QuantityGal,
				getdate()
				)
end
go
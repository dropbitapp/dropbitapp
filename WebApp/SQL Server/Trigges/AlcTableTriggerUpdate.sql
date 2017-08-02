-- ==============================================
-- Create dml trigger template Azure SQL Database 
-- ==============================================
-- Drop the dml trigger if it already exists
go
if object_id('alcohol_history_update', 'TR') is not null
drop trigger alcohol_history_update
go

create trigger alcohol_history_update 
   on  dbo.Alcohol_v3
   after update
as 
begin
	declare @PurchaseID int
	declare @ProductionID int
	declare @Alcohol nvarchar(50)
	declare @AlcoholID int
	declare @F2HID int

    -- Update statements for trigger here
	if(update(Value))

		select @Alcohol =  cast(inserted.Value as nvarchar(50)) from inserted
	
		select @AlcoholID = inserted.AlcoholID from inserted
	
		set @F2HID = (select F2HID from dbo.F2H where FName = 'Alcohol')
		
		if exists(select * from Purchase_v3 where @AlcoholID = AlcoholID)
			set @PurchaseID = (select PurchaseID from Purchase_v3 where @AlcoholID = AlcoholID)
			if(@PurchaseID > 0)	
				insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
				values (
				@PurchaseID,
				@F2HID,
				@Alcohol,
				getdate()
				)
		if exists(select * from Production_v3 where @AlcoholID = AlcoholID)
			set @ProductionID = (select ProductionID from Production_v3 where @AlcoholID = AlcoholID)
			if(@ProductionID > 0)	
				insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
				values (
				@ProductionID,
				@F2HID,
				@Alcohol,
				getdate()
				)
end
go
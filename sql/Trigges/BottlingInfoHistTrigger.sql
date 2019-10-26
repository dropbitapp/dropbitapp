-- ==============================================
-- Create dml trigger template Azure SQL Database 
-- ==============================================
-- Drop the dml trigger if it already exists
go
if object_id('bottlingInfo_history_update', 'TR') is not null
drop trigger bottlingInfo_history_update
go

create trigger bottlingInfo_history_update 
   on  dbo.BottlingInfo_v3
   after insert, update
as 
begin
	declare @ProductionID int
	declare @FieldValue nvarchar(50)

    -- Update statements for trigger here
	if(update(CaseCapacity))
		declare @FieldNameCaseCap nvarchar(50)
		set @FieldNameCaseCap = 'CaseCapacity'

		select @ProductionID =  cast(inserted.ProductionID as nvarchar(50)) from inserted

		select @FieldValue =  cast(inserted.CaseCapacity as nvarchar(50)) from inserted
			
		insert into dbo.BottlingInfoHist_v3 (ProductionID, FieldName, FieldValue, UpdateDate ) 
		values (
		@ProductionID,
		@FieldNameCaseCap,
		@FieldValue,
		getdate()
		)
		
	if(update(BottleVolume))
		declare @FieldNameBottleCap nvarchar(50)
		set @FieldNameBottleCap = 'BottleVolume'
		
		select @ProductionID =  cast(inserted.ProductionID as nvarchar(50)) from inserted

		select @FieldValue =  cast(inserted.BottleVolume as nvarchar(50)) from inserted
			
		insert into dbo.BottlingInfoHist_v3 (ProductionID, FieldName, FieldValue, UpdateDate ) 
		values (
		@ProductionID,
		@FieldNameBottleCap,
		@FieldValue,
		getdate()
		)

	if(update(CaseQuantity))
		declare @FieldNameCaseQuant nvarchar(50)
		set @FieldNameCaseQuant = 'CaseQuantity'
		
		select @ProductionID =  cast(inserted.ProductionID as nvarchar(50)) from inserted

		select @FieldValue =  cast(inserted.CaseQuantity as nvarchar(50)) from inserted
			
		insert into dbo.BottlingInfoHist_v3 (ProductionID, FieldName, FieldValue, UpdateDate ) 
		values (
		@ProductionID,
		@FieldNameCaseQuant,
		@FieldValue,
		getdate()
		)

	if(update(BottleQuantity))
		declare @FieldNameBottleQuant nvarchar(50)
		set @FieldNameBottleQuant = 'BottleQuantity'

		select @ProductionID =  cast(inserted.ProductionID as nvarchar(50)) from inserted

		select @FieldValue =  cast(inserted.BottleQuantity as nvarchar(50)) from inserted
			
		insert into dbo.BottlingInfoHist_v3 (ProductionID, FieldName, FieldValue, UpdateDate ) 
		values (
		@ProductionID,
		@FieldNameBottleQuant,
		@FieldValue,
		getdate()
		)	
end
go
-- ==============================================
-- Create dml trigger template Azure SQL Database 
-- ==============================================
-- Drop the dml trigger if it already exists
go
if object_id('blendedComponent_history_update', 'TR') is not null
drop trigger blendedComponent_history_update
go

create trigger blendedComponent_history_update 
   on  dbo.BlendedComponent_v3
   after insert, update
as 
begin

	declare @ProductionID int
	declare @Quantity nvarchar(50)
	declare @MaterialName nvarchar(50)
	declare @RecordId int -- this id is from material dictionary
	declare @UnitOfMeasurement nvarchar(50)
	

    -- Update statements for trigger here
	if(update(Quantity))

		select @ProductionID =  cast(inserted.ProductionID as nvarchar(50)) from inserted

		select @Quantity =  cast(inserted.Quantity as nvarchar(50)) from inserted
	
		select @RecordId = inserted.RecordId from inserted
		
		set @MaterialName = (select Name from dbo.MaterialDict_v3 where MaterialDictID = @RecordId)
		
		insert into dbo.BlendedComponentHist_v3 (ProductionID, FieldName, FieldValue, UpdateDate ) 
		values (
		@ProductionID,
		@MaterialName,
		@Quantity,
		getdate()
		)
		
	if(update(UnitOfMeasurement))

		select @ProductionID =  cast(inserted.ProductionID as nvarchar(50)) from inserted
	
		select @UnitOfMeasurement = inserted.UnitOfMeasurement from inserted
		
		insert into dbo.BlendedComponentHist_v3 (ProductionID, FieldName, FieldValue, UpdateDate ) 
		values (
		@ProductionID,
		'UnitOfMeasurement',
		@UnitOfMeasurement,
		getdate()
		)
end
go
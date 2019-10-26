go
if object_id('[production_history_update]', 'TR') is not null
drop trigger [production_history_update]
go

create trigger [dbo].[production_history_update] 
   on  [dbo].[Production_v3]
   after insert, update
as 
	declare @ProductionID int
	declare @ProductionName nvarchar(50)
	declare @ProductionTypeID int
	declare @ProductionTypeName nvarchar(50)
	declare @QuantityGal nvarchar(50)
	declare @QuantityGalID int
	declare @VolumeByWeightLB nvarchar(50)
	declare @VolumeByWeightLBID int
	declare @Alcohol nvarchar(50)
	declare @AlcoholID int
	declare @Proof nvarchar(50)
	declare @ProofID int
	declare @ProductionDate nvarchar(50)
	declare @Note nvarchar(50)
	declare @ProductionStartTime nvarchar(50)
	declare @ProductionEndTime nvarchar(50)
	declare @F2HID int
	declare @F2HValues nvarchar(50)
	declare @DistillerID int
	declare @Distiller nvarchar(50)
	declare @SpiritCut nvarchar(50)
	declare @SpiritType nvarchar(50)
	declare @F2HCurVal nvarchar(50)

-- if ProductionID column has been modified, it means that it is a new record, so we have to insert an entire row
if(update(ProductionID))	
begin
	select @ProductionID = inserted.ProductionID from inserted
	
	select @ProductionName = inserted.ProductionName from inserted
	
	select @ProductionTypeID = inserted.ProductionTypeID from inserted
	set @ProductionTypeName  = (select Name from dbo.ProductionType_v3 where ProductionTypeID = @ProductionTypeID)

	select @QuantityGalID = inserted.QuantityGalID from inserted
	set @QuantityGal = (select cast(Value as nvarchar(50)) from dbo.QuantityGal_v3 where QuantityGalID = @QuantityGalID)
		
	select @VolumeByWeightLBID = inserted.VolumeByWeightLBID from inserted
	set @VolumeByWeightLB = (select cast(Value as nvarchar(50)) from dbo.VolumeByWeightLB_v3 where VolumeByWeightLBID = @VolumeByWeightLBID)

	select @AlcoholID = inserted.AlcoholID from inserted
	set @Alcohol = (select cast(Value as nvarchar(50)) from dbo.Alcohol_v3 where AlcoholID = @AlcoholID)

	select @ProofID = inserted.ProofID from inserted
	set @Proof = (select cast(Value as nvarchar(50)) from dbo.Proof_v3 where ProofID = @ProofID)

	select @Note = cast(inserted.Note as nvarchar(50)) from inserted

	select @ProductionStartTime = inserted.ProductionStartTime from inserted

	select @ProductionEndTime = inserted.ProductionEndTime from inserted

	select @DistillerID = inserted.DistillerID from inserted
	set @Distiller = (select cast(Name as nvarchar(50)) from dbo.Distiller_v3 where DistillerID = @DistillerID)

	-- select F2HID where FName = '*'
	declare f2h_cursor cursor for select FName  from dbo.F2H
	open f2h_cursor
	fetch next from f2h_cursor into @F2HValues
	while (@@fetch_status = 0)
	begin
	set @F2HCurVal = 
	( case
		when @F2HValues = 'ProductionName'	then @ProductionName
		when @F2HValues = 'ProductionType'	then @ProductionTypeName
		when @F2HValues = 'Note'			then @Note
		when @F2HValues = 'State'			then null
		when @F2HValues = 'Status'			then null
		when @F2HValues = 'Distiller'		then @Distiller		
		when @F2HValues = 'QuantityGal'		then @QuantityGal
		when @F2HValues = 'VolumeByWeight'  then @VolumeByWeightLB
		when @F2HValues = 'Alcohol'			then @Alcohol
		when @F2HValues = 'Proof'			then @Proof
		when @F2HValues = 'SpiritCut'		then null
		when @F2HValues = 'SpiritType'		then null
		when @F2HValues = 'Storage'			then null
		when @F2HValues = 'ProductionStartTime'			then @ProductionStartTime
		when @F2HValues = 'ProductionEndTime'			then @ProductionEndTime	
		else ''
	end
	)
	
	set @F2HID = (select F2HID from dbo.F2H where @F2HValues = FName)
	if(@F2HID not in(1,2,3,4,5,10,15,16,17)) -- ignore these fields to avoid havin empty rows in history tables
		insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
		values (
		@ProductionID,
		@F2HID,
		@F2HCurVal,
		getdate()
		)
	fetch next from f2h_cursor into @F2HValues
	end
	close f2h_cursor
	deallocate f2h_cursor
end
	
else 
begin
	select @ProductionID = inserted.ProductionID from inserted

	if(update(ProductionName))
		select @ProductionName = inserted.ProductionName from inserted	
		
		set @F2HID = (select F2HID from dbo.F2H where FName = 'ProductionName')

		insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
		values (
		@ProductionID,
		@F2HID,
		@ProductionName,
		getdate()
		)
	
	if(update(ProductionTypeID))
		select @ProductionTypeID = inserted.ProductionTypeID from inserted
		set @ProductionTypeName  = (select Name from dbo.ProductionType_v3 where ProductionTypeID = @ProductionTypeID)

		set @F2HID = (select F2HID from dbo.F2H where FName = 'ProductionType')

		insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
		values (
		@ProductionID,
		@F2HID,
		@ProductionTypeName,
		getdate()
		)

	if(update(ProductionStartTime))
		select @ProductionStartTime = cast(inserted.ProductionStartTime as nvarchar(50)) from inserted
				
		set @F2HID = (select F2HID from dbo.F2H where FName = 'ProductionStartTime')

		insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
		values (
		@ProductionID,
		@F2HID,
		@ProductionStartTime,
		getdate()
		)

	if(update(ProductionStartTime))
		select @ProductionEndTime = cast(inserted.ProductionEndTime as nvarchar(50)) from inserted
				
		set @F2HID = (select F2HID from dbo.F2H where FName = 'ProductionEndTime')

		insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
		values (
		@ProductionID,
		@F2HID,
		@ProductionEndTime,
		getdate()
		)
	if(update(Note))
		select @Note = inserted.Note from inserted
		
		set @F2HID = (select F2HID from dbo.F2H where FName = 'Note')

		insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
		values (
		@ProductionID,
		@F2HID,
		@Note,
		getdate()
		)
end
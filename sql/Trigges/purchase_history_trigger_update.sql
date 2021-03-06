USE [DevV2]
GO
/****** Object:  Trigger [dbo].[purchase_history_update]    Script Date: 3/6/2017 10:30:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER trigger [dbo].[purchase_history_update] 
   on  [dbo].[Purchase_v3]
   after insert, update
as 
	declare @PurchaseID int
	declare @PurchaseName nvarchar(50)
	declare @PurchaseTypeName nvarchar(50)
	declare @PurchaseTypeID int
	declare @MaterialDictName nvarchar(50)
	declare @MaterialDictID int
	declare @Price nvarchar(50)
	declare @VendorID int
	declare @VendorName nvarchar(50)
	declare @QuantityGal nvarchar(50)
	declare @QuantityGalID int
	declare @VolumeByWeightLB nvarchar(50)
	declare @VolumeByWeightLBID int
	declare @Alcohol nvarchar(50)
	declare @AlcoholID int
	declare @Proof nvarchar(50)
	declare @ProofID int
	--declare @UnitOfMeasurement nvarchar(50)
	--declare @UnitOfMeasurementID int
	declare @PurchaseDate nvarchar(50)
	declare @Note nvarchar(50)
	declare @State nvarchar(50)
	declare @StateID int
	declare @Status nvarchar(50)
	declare @StatusID int
	declare @F2HID int
	declare @F2HValues nvarchar(50)
	declare @DistillerID int
	declare @Distiller nvarchar(50)
	declare @F2HCurVal nvarchar(50)

-- if PurchaseID column has been modified, it means that it is a new record, so we have to insert an entire row
if(update(PurchaseID))
begin
	
	select @PurchaseID = inserted.PurchaseID from inserted
	
	select @PurchaseName = inserted.PurchaseName from inserted
	
	select @PurchaseTypeID = inserted.PurchaseTypeID from inserted
	set @PurchaseTypeName  = (select Name from dbo.PurchaseType_v3 where PurchaseTypeID = @PurchaseTypeID)


	select @MaterialDictID = inserted.MaterialDictID from inserted

	set @MaterialDictName  = (select Name from dbo.MaterialDict_v3 where MaterialDictID = @MaterialDictID)

	select @Price = cast(inserted.Price as nvarchar(50)) from inserted

	select @VendorID = inserted.VendorID from inserted
	set @VendorName = (select Name from dbo.Vendor_v3 where VendorID = @VendorID)

	select @QuantityGalID = inserted.QuantityGalID from inserted
	set @QuantityGal = (select cast(Value as nvarchar(50)) from dbo.QuantityGal_v3 where QuantityGalID = @QuantityGalID)
		
	select @VolumeByWeightLBID = inserted.VolumeByWeightLBID from inserted
	set @VolumeByWeightLB = (select cast(Value as nvarchar(50)) from dbo.VolumeByWeightLB_v3 where VolumeByWeightLBID = @VolumeByWeightLBID)

	select @AlcoholID = inserted.AlcoholID from inserted
	set @Alcohol = (select cast(Value as nvarchar(50)) from dbo.Alcohol_v3 where AlcoholID = @AlcoholID)

	select @ProofID = inserted.ProofID from inserted
	set @Proof = (select cast(Value as nvarchar(50)) from dbo.Proof_v3 where ProofID = @ProofID)

	--select @UnitOfMeasurementID = inserted.UnitOfMeasurementID from inserted
	--set @UnitOfMeasurement = (select cast(Value as nvarchar(50)) from dbo.UnitOfMeasurement_v3 where UnitOfMeasurementID = @UnitOfMeasurementID)

	select @Note = cast(inserted.Note as nvarchar(50)) from inserted

	select @StateID = inserted.StateID from inserted
	set @State = (select cast(Name as nvarchar(50)) from dbo.State_v3 where StateID = @StateID)

	select @StatusID = inserted.StatusID from inserted
	set @Status = (select cast(Name as nvarchar(50)) from dbo.Status_v3 where StatusID = @StatusID)

	select @PurchaseDate = cast(inserted.PurchaseDate as nvarchar(50)) from inserted

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
		when @F2HValues = 'PurchaseName'	then @PurchaseName
		when @F2HValues = 'PurchaseType'	then @PurchaseTypeName
		when @F2HValues = 'MaterialDict'	then @MaterialDictName
		when @F2HValues = 'Price'			then @Price
		when @F2HValues = 'Vendor'			then @VendorName
		when @F2HValues = 'PurchaseDate'	then @PurchaseDate
		when @F2HValues = 'Note'			then @Note
		when @F2HValues = 'State'			then @State
		when @F2HValues = 'Status'			then @Status
		when @F2HValues = 'Distiller'		then @Distiller		
		when @F2HValues = 'QuantityGal'		then @QuantityGal
		when @F2HValues = 'VolumeByWeight'  then @VolumeByWeightLB
		when @F2HValues = 'Alcohol'			then @Alcohol
		when @F2HValues = 'Proof'			then @Proof
		when @F2HValues = 'Storage'			then null
		else ''
	end
	)
	
	set @F2HID = (select F2HID from dbo.F2H where @F2HValues = FName)
	if(@F2HID not in(15)) -- ignore these fields to avoid havin empty rows in history tables
		insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
		values (
		@PurchaseID,
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
	select @PurchaseID = inserted.PurchaseID from inserted

	if(update(PurchaseName))
	begin	
		select @PurchaseName = inserted.PurchaseName from inserted	
		
		set @F2HID = (select F2HID from dbo.F2H where FName = 'PurchaseName')

		insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
		values (
		@PurchaseID,
		@F2HID,
		@PurchaseName,
		getdate()
		)
	end

	if(update(PurchaseTypeID))
	begin
		select @PurchaseTypeID = inserted.PurchaseTypeID from inserted
		set @PurchaseTypeName  = (select Name from dbo.PurchaseType_v3 where PurchaseTypeID = @PurchaseTypeID)

		set @F2HID = (select F2HID from dbo.F2H where FName = 'PurchaseType')

		insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
		values (
		@PurchaseID,
		@F2HID,
		@PurchaseTypeName,
		getdate()
		)
	end

	if(update(MaterialDictID))
	begin
		select @MaterialDictID = inserted.MaterialDictID from inserted
		set @MaterialDictName  = (select Name from dbo.MaterialDict_v3 where MaterialDictID = @MaterialDictID)

		set @F2HID = (select F2HID from dbo.F2H where FName = 'MaterialDict')

		insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
		values (
		@PurchaseID,
		@F2HID,
		@MaterialDictName,
		getdate()
		)
	end

	if(update(Price))
	begin
		select @Price = cast(inserted.Price as nvarchar(50)) from inserted
		
		set @F2HID = (select F2HID from dbo.F2H where FName = 'Price')

		insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
		values (
		@PurchaseID,
		@F2HID,
		@Price,
		getdate()
		)
	end

	if(update(VendorID))
	begin		
		select @VendorID = inserted.VendorID from inserted
		set @VendorName = (select Name from dbo.Vendor_v3 where VendorID = @VendorID)
		
		set @F2HID = (select F2HID from dbo.F2H where FName = 'Vendor')

		insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
		values (
		@PurchaseID,
		@F2HID,
		@VendorName,
		getdate()
		)
	end

	if(update(PurchaseDate))
	begin
		select @PurchaseDate = cast(inserted.PurchaseDate as nvarchar(50)) from inserted
				
		set @F2HID = (select F2HID from dbo.F2H where FName = 'PurchaseDate')

		insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
		values (
		@PurchaseID,
		@F2HID,
		@PurchaseDate,
		getdate()
		)
	end

	if(update(Note))
	begin
		select @Note = inserted.Note from inserted
		
		set @F2HID = (select F2HID from dbo.F2H where FName = 'Note')

		insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
		values (
		@PurchaseID,
		@F2HID,
		@Note,
		getdate()
		)
	end
	
	if(update(StateID))
	begin
		select @StateID = inserted.StateID from inserted
		set @State = (select cast(Name as nvarchar(50)) from dbo.State_v3 where StateID = @StateID)
	end

	if(update(StatusID))
	begin
		select @StatusID = inserted.StatusID from inserted
		set @Status = (select cast(Name as nvarchar(50)) from dbo.Status_v3 where StatusID = @StatusID)
		
		set @F2HID = (select F2HID from dbo.F2H where FName = 'Status')

		insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
		values (
		@PurchaseID,
		@F2HID,
		@State,
		getdate()
		)
	end
end

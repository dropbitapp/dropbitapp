-- ==============================================
-- Create dml trigger template Azure SQL Database 
-- ==============================================
-- Drop the dml trigger if it already exists
go
if object_id('proof_history_update', 'TR') is not null
drop trigger proof_history_update
go

create trigger proof_history_update 
   on  dbo.Proof_v3
   after update
as 
begin

	declare @PurchaseID int
	declare @ProductionID int
	declare @Proof nvarchar(50)
	declare @ProofID int
	declare @F2HID int

    -- Update statements for trigger here
	if(update(Value))

		select @Proof =  cast(inserted.Value as nvarchar(50)) from inserted
	
		select @ProofID = inserted.ProofID from inserted
	
		set @F2HID = (select F2HID from dbo.F2H where FName = 'Proof')
		
		if exists(select * from Purchase_v3 where @ProofID = ProofID)
			set @PurchaseID = (select PurchaseID from Purchase_v3 where @ProofID = ProofID)
			if(@PurchaseID > 0)	
				insert into dbo.PurchaseHist_v3 (PurchaseID, F2HID, F2HValue, UpdateDate) 
				values (
				@PurchaseID,
				@F2HID,
				@Proof,
				getdate()
				)
		if exists(select * from Production_v3 where @ProofID = ProofID)
			set @ProductionID = (select ProductionID from Production_v3 where @ProofID = ProofID)
			if(@ProductionID > 0)	
				insert into dbo.ProductionHist_v3 (ProductionID, F2HID, F2HValue, UpdateDate) 
				values (
				@ProductionID,
				@F2HID,
				@Proof,
				getdate()
				)
		end
go
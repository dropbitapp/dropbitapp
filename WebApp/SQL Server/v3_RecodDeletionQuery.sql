
--*** ensure that table names have not been changed before this query is being RUN***

-- Cleaning up production records routine
declare @VolumeByWeightLBID int
declare @QuantityGalID int
declare @AlcoholID int
declare @ProofID int
declare @tempProdID int

-- remove records from VolumeByWeight table
declare vbw_cursor cursor for select VolumeByWeightLBID from dbo.Production_v3
open vbw_cursor
fetch next from vbw_cursor into @VolumeByWeightLBID
while (@@fetch_status = 0)
begin
delete from dbo.VolumeByWeightLB_v3 where VolumeByWeightLBID = @VolumeByWeightLBID
fetch next from vbw_cursor into @VolumeByWeightLBID
end
close vbw_cursor
deallocate vbw_cursor

-- remove records from VolQuantityGal table
declare quantGal_cursor cursor for select QuantityGalID from Production_v3
open quantGal_cursor
fetch next from quantGal_cursor into @QuantityGalID
while (@@fetch_status = 0)
begin
delete from dbo.QuantityGal_v3 where QuantityGalID = @QuantityGalID
fetch next from quantGal_cursor into @QuantityGalID
end
close quantGal_cursor
deallocate quantGal_cursor

-- remove records from Alcohol table
declare alcohol_cursor cursor for select AlcoholID from Production_v3
open alcohol_cursor
fetch next from alcohol_cursor into @QuantityGalID
while (@@fetch_status = 0)
begin
delete from dbo.Alcohol_v3 where AlcoholID = @AlcoholID
fetch next from alcohol_cursor into @AlcoholID
end
close alcohol_cursor
deallocate alcohol_cursor

-- remove records from Proof table
declare proof_cursor cursor for select ProofID from Production_v3
open proof_cursor
fetch next from proof_cursor into @ProofID
while (@@fetch_status = 0)
begin
delete from dbo.Proof_v3 where ProofID = @ProofID
fetch next from proof_cursor into @ProofID
end
close proof_cursor
deallocate proof_cursor

-- remove records from Production table and tables that use its primary key which is ProductionID
declare prodT_cursor cursor for select ProductionID from Production_v3
open prodT_cursor
fetch next from prodT_cursor into @tempProdID
while (@@fetch_status = 0)
begin
delete from Production_v3 where ProductionID = @tempProdID
delete from Avail4Bottling_v3 where RecordId = @tempProdID
delete from dbo.ProductionToPurchase_v3 where ProductionID = @tempProdID
delete from dbo.InvDistillable_v3 where RecordId = @tempProdID AND TableIdentifier = 'prod'
delete from dbo.Avail4Blending_v3 where RecordId = @tempProdID
delete from dbo.StorageToRecord_v3 where RecordId = @tempProdID AND TableIdentifier = 'prod'
delete from dbo.ProductionToSpirit_v3 where ProductionID = @tempProdID
delete from dbo.ProductionToSpiritCut_v3 where ProductionID = @tempProdID
delete from dbo.BlendedComponent_v3 where ProductionID = @tempProdID
delete from dbo.BottlingInfo_v3 where ProductionID = @tempProdID
fetch next from prodT_cursor into @tempProdID
end
close prodT_cursor
deallocate prodT_cursor

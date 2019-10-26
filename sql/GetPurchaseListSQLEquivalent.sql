-- PurchaseToRawMaterial (identifierId) {PurchaseId, PurchaseToRawMaterialId, RawMaterialId}
-- RawMaterial (RawMaterialId) {RawMaterialName}
-- Quantity (RecordId, IdentifierId) {QuantityValue}
-- RecordToStorage (RecordId, IdentifierId) {StorageId}
-- Storage (StorageId) {StorageName}
-- Note (RecordId, IdentifierId) {NoteValue}
-- Purchase (PurchaseId) {PurchaseDate}
-- Price (PurchaseId) {PriceValue}
-- PurchaseToVendor (PurchaseId) {VendorId}
-- Vendor (VendorId) {VendorName}
-- VolumeByWeight (RecordId, IdentifierId) {VolumeByWeightValue}
-- AlcoholContent (RecordId, IdentifierId) {AlcContValue}
-- ProofGallon (RecordId, IdentifierId) {ProofGallonValue}


-- This Routine isused to get Fermented, Supply and Additive

-- This Routine isused to get Fermentable, Supply and Additive
select 
PurToRawMat.PurchaseId, PurToRawMat.PurchaseToRawMaterialId, PurToRawMat.RawMaterialId,-- PurToRawMat.IdentifierId as purToRMatIdent, 
RawMat.RawMaterialName
,ISNULL(Quant.QuantityValue,0) as Quantity--, Quant.IdentifierId as quantIdent
,ISNULL(Store.StorageName, '') as [Storage Name]
,ISNULL(Store.StorageId, 0) as [StorageId]
--,Store.StorageId as [StorageId]
--,ISNULL(RecToStor.RecordToStorageId, 0) as storageId--, RecToStor.IdentifierId as RecToStorIdentifierId
,note.NoteValue--, note.IdentifierId as noteIdentofierId
,PurInfo.PurchaseDate
,price.PriceValue
,PurToVendor.VendorId
,vendor.VendorName
,ISNULL(VBW.VolumeByWeightValue, 0) as VBW
,ISNULL(Alcohol.AlcContValue, 0) as AlcoholCont
,ISNULL(Proof.ProofGallonValue, 0) as ProofCont
from dbo.PurchaseToRawMaterial as PurToRawMat
join dbo.RawMaterial as RawMat 
	on PurToRawMat.RawMaterialId = RawMat.RawMaterialId
left join dbo.Quantity as Quant
	on PurToRawMat.PurchaseToRawMaterialId = Quant.RecordId AND PurToRawMat.IdentifierId = Quant.IdentifierId
left join dbo.RecordToStorage as RecToStor 
	on  PurToRawMat.PurchaseToRawMaterialId = RecToStor.RecordId AND PurToRawMat.IdentifierId = RecToStor.IdentifierId
left join dbo.Storage as Store
	on RecToStor.StorageId = Store.StorageId
left join dbo.Note as note
	on  PurToRawMat.PurchaseToRawMaterialId = note.RecordId AND PurToRawMat.IdentifierId = note.IdentifierId
left join dbo.Purchase as PurInfo
	on PurToRawMat.PurchaseId = PurInfo.PurchaseId
left join dbo.Price as price
	on PurToRawMat.PurchaseId = price.PurchaseId
left join dbo.PurchaseToVendor as PurToVendor
	on PurToRawMat.PurchaseId = PurToVendor.PurchaseId
left join dbo.Vendor as vendor
	on PurToVendor.VendorId = vendor.VendorId
left join dbo.VolumeByWeight as VBW
	on PurToRawMat.PurchaseToRawMaterialId = VBW.RecordId AND PurToRawMat.IdentifierId = VBW.IdentifierId
left join dbo.AlcoholContent as Alcohol
	on PurToRawMat.PurchaseToRawMaterialId = Alcohol.RecordId AND PurToRawMat.IdentifierId = Alcohol.IdentifierId
left join dbo.ProofGallon as Proof
	on PurToRawMat.PurchaseToRawMaterialId = Proof.RecordId AND PurToRawMat.IdentifierId = Proof.IdentifierId
where PurToRawMat.IdentifierId = 1
select 
rawM.RawMaterialId, rawM.RawMaterialName
,ISNULL(note.NoteValue, '') as [Note]
,ISNULL(note.NoteId, 0) as [NoteId]
,ISNULL(unitToRec.RecordItemToUnitOfMeasurementId, 0) as [RecordItemToUnitOfMeasurementId]
,ISNULL(unitToRec.UnitOfMeasurementId, 0)as [UnitId]
,ISNULL(units.UnitName, '') as UnitName
from dbo.PurchaseMaterialType AS PMT
left join dbo.PurchaseMaterialTypeToRawMaterial AS PMTRM on  PMT.PurchaseMaterialTypeId = PMTRM.PurchaseMaterialTypeId
left join  dbo.RawMaterial as rawM on rawM.RawMaterialId = PMTRM.RawMaterialId
left join dbo.Note as note on note.RecordId = PMTRM.RawMaterialId
left join dbo.RecordItemToUnitOfMeasurement as unitToRec on unitToRec.RecordId = PMTRM.RawMaterialId
left join dbo.UnitOfMeasurement as units on units.UnitOfMeasurementId = unitToRec.UnitOfMeasurementId

where PMT.PurchaseMaterialTypeName = 'Additive'
--where note.IdentifierId = 1 AND PMT.PurchaseMaterialTypeName = 'Fermented'

/*
rawM.RawMaterialId, rawM.RawMaterialName
,ISNULL(note.NoteValue, '') as [Note]
,ISNULL(note.NoteId, 0) as [NoteId]
,ISNULL(unitToRec.RecordItemToUnitOfMeasurementId, 0) as [RecordItemToUnitOfMeasurementId]
--,ISNULL(unitToRec.RecordItemName, '')  
,ISNULL(unitToRec.UnitOfMeasurementId, 0)as [UnitId]
,ISNULL(units.UnitName, '') as UnitName
*/
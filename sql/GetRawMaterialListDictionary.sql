select
rawM.RawMaterialId, rawM.RawMaterialName
,ISNULL(note.NoteValue, '') as [Note]
,ISNULL(note.NoteId, 0) as [NoteId]
,ISNULL(unitToRec.RecordItemToUnitOfMeasurementId, 0) as [RecordItemToUnitOfMeasurementId]
--,ISNULL(unitToRec.RecordItemName, '')  
,ISNULL(unitToRec.UnitOfMeasurementId, 0)as [UnitId]
,ISNULL(units.UnitName, '') as UnitName
from dbo.RawMaterial as rawM 
left join dbo.Note as note on note.RecordId = rawM.RawMaterialId
left join dbo.RecordItemToUnitOfMeasurement as unitToRec on unitToRec.RecordId = rawM.RawMaterialId
left join dbo.UnitOfMeasurement as units on units.UnitOfMeasurementId = unitToRec.UnitOfMeasurementId

where rawM.IdentifierId = 1


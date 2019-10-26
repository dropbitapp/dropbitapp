-- this was pre v3 db architecture

select * 

from  dbo.Distilled as distilledT
left join dbo.Quantity as quant on distilledT.DistilledId = quant.RecordId and quant.IdentifierId = 15
left join dbo.RecordToStorage as recToStorageT on distilledT.DistilledId = recToStorageT.RecordId and recToStorageT.IdentifierId = 15
left join dbo.DistilledToSpiritCut as distilToSpiritCut on distilledT.DistilledId = distilToSpiritCut.DistilledId
left join dbo.Note as note on distilledT.DistilledId = note.RecordId and note.IdentifierId = 15
left join dbo.Distillation as distillation on distilledT.DistilledId = distillation.DistilledId
left join dbo.Storage as storageT on recToStorageT.StorageId = storageT.StorageId
left join dbo.AlcoholContent as alcT on distilledT.DistilledId = alcT.RecordId and alcT.IdentifierId = 15
left join dbo.ProofGallon as proofT on distilledT.DistilledId = proofT.RecordId and proofT.IdentifierId = 15
left join dbo.VolumeByWeight as vbwT on distilledT.DistilledId = vbwT.RecordId and vbwT.IdentifierId = 15
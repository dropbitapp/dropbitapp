-- used to populate Raw Material list boxes in Fermentation Produciton workflow

select 
rec.RecordId [PurchaseID]
, ISNULL(purch.PurchaseName, '') [PurchaseBatchName]
, ISNULL(materials.Name, '') [RawMaterialName]
, materials.MaterialDictID

from dbo.InvFermentable_v3 as rec
left join dbo.Purchase_v3 as purch on rec.RecordId = purch.PurchaseID
left join dbo.MaterialDict_v3 as materials on purch.MaterialDictID = materials.MaterialDictID
where rec.TableIdentifier = 'pur' --&& rec.DistillerID == DistillerID

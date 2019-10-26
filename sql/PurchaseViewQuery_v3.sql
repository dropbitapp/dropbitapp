select
purchT.PurchaseName
, purchT.Price
, purchT.PurchaseDate
, purchT.Note [PurchaseNote]
, purType.Name [PurchaseType]
, ISNULL(material.Name, '') [MaterialName]
, vendor.Name [VendorName]
, ISNULL(galQuant.Value, 0) [Gallons]
, ISNULL(VBW.Value, 0) [VolumeByWeight]
, ISNULL(alc.Value, 0) [Alcohol]
, ISNULL(proof.Value, 0) [Proof]
, ISNULL(states.Name, '') [State]
, ISNULL(statuses.Name, '') [Status]
from dbo.Purchase_v3 as purchT
left join dbo.PurchaseType_v3 as purType on purchT.PurchaseTypeID = purType.PurchaseTypeID
left join dbo.MaterialDict_v3 as material on purchT.MaterialDictID = material.MaterialDictID
left join dbo.Vendor_v3 as vendor on purchT.VendorID = vendor.VendorId
left join dbo.QuantityGal_v3 as galQuant on purchT.QuantityGalID = galQuant.QuantityGalID
left join dbo.VolumeByWeightLB_v3 as VBW on purchT.VolumeByWeightLBID = VBW.VolumeByWeightLBID
left join dbo.Alcohol_v3 as alc on purchT.AlcoholID = alc.AlcoholID
left join dbo.Proof_v3 as proof on purchT.ProofID = proof.ProofID
left join dbo.State_v3 as states on purchT.StateID = states.StateID
left join dbo.Status_v3 as statuses on purchT.StatusID = statuses.StatusID
where purType.Name = 'Fermentable'
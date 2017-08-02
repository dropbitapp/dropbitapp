select 
purch.PurchaseID [PurchaseID],
purch.PurchaseName [PurchaseBatchName],
purch.StatusID [StatusID],
matDic.Name [RawMaterialName],
matDic.MaterialDictID [MaterialDictID],
quant.Value [Quantity],
vbw.Value [VolumeByWeight]

from dbo.Purchase_v3 as purch
left join dbo.QuantityGal_v3 as quant on purch.QuantityGalID = quant.QuantityGalID
left join dbo.VolumeByWeightLB_v3 as vbw on purch.VolumeByWeightLBID = vbw.VolumeByWeightLBID
left join dbo.MaterialDict_v3 as matDic on purch.MaterialDictID = matDic.MaterialDictID
left join dbo.AspNetUsersToDistillers_v3 as distiller on purch.DistillerID = distiller.DistillerID
left join dbo.Status_v3 as status on purch.StatusID = status.StatusID

where (purch.StatusID = 1 or purch.StatusID = 2)and distiller.userID = UserId 
-- Purchase
select
purch.PurchaseID [PurchaseID],
purch.PurchaseName [PurchaseBatchName],
purch.StatusID [StatusID],
purch.StateID [StateID],
purch.BurningDownMethod [BurningDownMethod],
quant.Value [Quantity],
vbw.Value [VolumeByWeight],


from Purchase_v3 as purch
left join AspNetUsersToDistillers_v3 as distillers on purch.DistillerID = distillers.DistillerID
left join dbo.QuantityGal_v3 as quant on purch.QuantityGalID = quant.QuantityGalID
left join dbo.VolumeByWeightLB_v3 as vbw on purch.VolumeByWeightLBID = vbw.VolumeByWeightLBID
left join dbo.Status_v3 as status on purch.StatusID = status.StatusID

where (purch.StatusID = 1 or purch.StatusID = 2) and (purch.StateID = 2 or purch.StateID = 3)and distillers.userID = 1--UserId

-- Production
select
prod.ProductionID [ProductionID],
prod.ProductionName [ProductionName],
prod.StatusID [StatusID],
prod.StateID [StateID],
prod.BurningDownMethod [BurningDownMethod],
quant.Value [Quantity],
vbw.Value [VolumeByWeight]


from Production_v3 as prod
left join AspNetUsersToDistillers_v3 as distillers on prod.DistillerID = distillers.DistillerID
left join dbo.QuantityGal_v3 as quant on prod.QuantityGalID = quant.QuantityGalID
left join dbo.VolumeByWeightLB_v3 as vbw on prod.VolumeByWeightLBID = vbw.VolumeByWeightLBID
left join dbo.Status_v3 as status on prod.StatusID = status.StatusID

where (prod.StatusID = 1 or prod.StatusID = 2) and
 (prod.StateID = 2 or prod.StateID = 3)and distillers.userID = 1--UserId
-- part 6 of the report is tricky (at least for my knowledge os t-sql), we need to do this in two steps
-- step 1 get material amounts and where possible, also get ProductionReportMaterialCategoryID
-- step 2 get ProductionReportMaterialCategoryID for the records above that didn't have them

-- step 1 get material amounts and where possible, also get ProductionReportMaterialCategoryID
select
prod.ProductionID,
matDict.Name [Material Dict Name],
prodC.ContentValue [Value],
contF.ContentFieldID [ContentFieldID],
prodRepMatCat.ProductionReportMaterialCategoryID

from ProductionContent as prodCont
left join Production  as prod on prodCont.ProductionID = prod.ProductionID 
left join AspNetUserToDistiller as distillers on prod.DistillerID = distillers.DistillerId
left join ProductionContent as prodC on prod.ProductionID = prodC.ProductionID
left join ProductionToPurchase as prod2Purch on prod.ProductionID = prod2Purch.ProductionID
left join Purchase as purch on prod2Purch.PurchaseID = purch.PurchaseID
left join MaterialDict as matDict on purch.MaterialDictID = matDict.MaterialDictID
left join ContentField as contF on prodC.ContentFieldID = contF.ContentFieldID
left join ProductionToSpiritTypeReporting as prod2SpiritType on prod.ProductionID = prod2SpiritType.ProductionID
left join MaterialKindReporting as matKindRep on prod2SpiritType.MaterialKindReportingID = matKindRep.MaterialKindReportingID
left join ProdRepMatCat2MaterialKind as prodRepMatCat2MatKind on matKindRep.MaterialKindReportingID = prodRepMatCat2MatKind.MaterialKindReportingID
left join ProductionReportMaterialCategory as prodRepMatCat on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID = prodRepMatCat.ProductionReportMaterialCategoryID
where distillers.UserId = 1  and prod.ProductionEndTime >= '07/01/2016' and prod.ProductionEndTime <= '08/01/2017' and prodC.isProductionComponent = 0  and purch.PurchaseTypeID != 3--and prodRepMatCat.ProductionReportMaterialCategoryID is not null

-- step 2 get ProductionReportMaterialCategoryID for the records above that didn't have them.
select 
prodRepMatCat.ProductionReportMaterialCategoryID
from ProductionContent as prodContent 
left join ProductionToSpiritTypeReporting as prod2SpiritType on prodContent.ProductionID = prod2SpiritType.ProductionID
left join MaterialKindReporting as matKindRep on prod2SpiritType.MaterialKindReportingID = matKindRep.MaterialKindReportingID
left join ProdRepMatCat2MaterialKind as prodRepMatCat2MatKind on matKindRep.MaterialKindReportingID = prodRepMatCat2MatKind.MaterialKindReportingID
left join ProductionReportMaterialCategory as prodRepMatCat on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID = prodRepMatCat.ProductionReportMaterialCategoryID
where prodContent.isProductionComponent = 1 and prodContent.RecordID in (52,55,102,54,53)




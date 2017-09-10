-- Production report parts 1 through 5 related queries

select distinct
prod.Gauged,
prod.StateID,
prod.ProductionID,
spiritTypeRep.ProductTypeName [Spirit Short Name],
prodReport.Redistilled [Redistilled],
matKindRep.MaterialKindName [Material Name],

(prodReport.Weight) [Weight],
(prodReport.Volume) [Volume],
(prodReport.Alcohol) [Alcohol],
(prodReport.Proof) [Proof],
spiritTypeRep.SpiritTypeReportingID,
matKindRep.MaterialKindReportingID,
prodRepMatCat.MaterialCategoryName,
prodRepMatCat.ProductionReportMaterialCategoryID

from Production as prod
left join Production4Reporting as prodReport on prod.ProductionID = prodReport.ProductionID
left join AspNetUserToDistiller as distillers on prod.DistillerID = distillers.DistillerId
left join ProductionToSpiritTypeReporting as prod2SpiritType on prod.ProductionID = prod2SpiritType.ProductionID
left join MaterialKindReporting as matKindRep on matKindRep.MaterialKindReportingID = prod2SpiritType.MaterialKindReportingID
left join SpiritTypeReporting as spiritTypeRep on spiritTypeRep.SpiritTypeReportingID = prod2SpiritType.SpiritTypeReportingID
left join ProductionToPurchase as prod2Purch on prod2Purch.ProductionID = prod.ProductionID
left join Purchase4Reporting as purch4Reprt on prod2Purch.PurchaseID = purch4Reprt.PurchaseID
left join ProdRepMatCat2MaterialKind as prodRepMatCat2MatKind on matKindRep.MaterialKindReportingID = prodRepMatCat2MatKind.MaterialKindReportingID
left join ProductionReportMaterialCategory as prodRepMatCat on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID = prodRepMatCat.ProductionReportMaterialCategoryID
where prod.Gauged = 0 and distillers.UserId = 1 and (prod.StatusID = 1 or prod.StatusID = 2) and prod.StateID in (4,5) and prod.ProductionEndTime >= '07/01/2016' and prod.ProductionEndTime <= '08/01/2017'
       
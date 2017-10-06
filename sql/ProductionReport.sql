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
where prod.Gauged = 1 and distillers.UserId = 1 and (prod.StatusID = 1 or prod.StatusID = 2) and prod.StateID in (4,5) and prod.ProductionEndTime >= '10/01/2017' and prod.ProductionEndTime <= '10/30/2017'
       


-- Part 1

-- this query tracks Proof Gallons that came in for redistillation
declare @received4redistillation nvarchar(64) = 'p1_15';
-- Get Production records
select distinct
@received4redistillation [ReportRowIdentifier],
prod.ProductionID,
spiritTypeRep.ProductTypeName [Spirit Short Name],
NULL [Material Name],
NULL [Weight],
NULL [Volume],
NULL [Alcohol],
prodContent.ContentValue [Proof],
spiritTypeRep.SpiritTypeReportingID,
prodContent.ContentFieldID 

from Production as prod
left join ProductionContent as prodContent on prod.ProductionID = prodContent.ProductionID
left join AspNetUserToDistiller as distillers on prod.DistillerID = distillers.DistillerId
left join ProductionToSpiritTypeReporting as prod2SpiritType on prodContent.RecordID = prod2SpiritType.ProductionID
left join SpiritTypeReporting as spiritTypeRep on spiritTypeRep.SpiritTypeReportingID = prod2SpiritType.SpiritTypeReportingID
where 
prod.ProductionEndTime between CONVERT(DATE, '2017-10-01') and CONVERT(DATE, '2017-10-31')
and prod.Gauged = 1 
and distillers.UserId = 1 
and (prod.StatusID = 1 or prod.StatusID = 2) 
and prodContent.ContentFieldID in (16, 18, 20, 22) 
and spiritTypeRep.SpiritTypeReportingID is not NULL
       
union
-- Get Purchases
select distinct
@received4redistillation [ReportRowIdentifier],
prod.ProductionID,
spiritTypeRep.ProductTypeName [Spirit Short Name],
NULL [Material Name],
NULL [Weight],
NULL [Volume],
NULL [Alcohol],
prodContent.ContentValue [Proof],
spiritTypeRep.SpiritTypeReportingID,
prodContent.ContentFieldID 

from Production as prod
left join ProductionContent as prodContent on prod.ProductionID = prodContent.ProductionID
left join AspNetUserToDistiller as distillers on prod.DistillerID = distillers.DistillerId
left join PurchaseToSpiritTypeReporting as pur2SpiritType on prodContent.RecordID = pur2SpiritType.PurchaseID
left join SpiritTypeReporting as spiritTypeRep on spiritTypeRep.SpiritTypeReportingID = pur2SpiritType.SpiritTypeReportingID
where 
prod.ProductionEndTime between CONVERT(DATE, '2017-10-01') and CONVERT(DATE, '2017-10-31')
and prod.Gauged = 1 
and distillers.UserId = 1 
and (prod.StatusID = 1 or prod.StatusID = 2) 
and prodContent.ContentFieldID in (16, 18, 20, 22) 
and spiritTypeRep.SpiritTypeReportingID is not NULL
       
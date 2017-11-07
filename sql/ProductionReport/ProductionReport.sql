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
where prod.Gauged = 1 and distillers.UserId = 7 and (prod.StatusID = 1 or prod.StatusID = 2) and prod.StateID in (3,4,5) and prod.ProductionEndTime >= '09/01/2017' and prod.ProductionEndTime <= '09/30/2017'
       

-- Line 15
declare @received4redistillation nvarchar(64) = 'p1_15';
-- let's get initial set of ProducitonID for which we will be looking for records that went into it
select distinct
--@received4redistillation [ReportRowIdentifier],
prod.ProductionID [Resulting Production ID],
prodContent.isProductionComponent [isProductionComponent],
CF.ContentFieldName [ContentFieldName],
prodContent.RecordID,
prodContent.isProductionComponent,
prodContent.ContentValue

from Production as prod
left join ProductionContent as prodContent on prod.ProductionID = prodContent.ProductionID
left join AspNetUserToDistiller as distillers on prod.DistillerID = distillers.DistillerId
left join ContentField as CF on prodContent.ContentFieldID = CF.ContentFieldID
where prod.Gauged = 1 
and distillers.UserId = 7 
and prod.StateID = 3 
and ProductionEndTime between CONVERT(DATE, '2017-09-01') and CONVERT(DATE, '2017-09-30')
and prodContent.ContentFieldID in (16, 18, 20, 22)

-- not that we got list of ids, we can iterate through them.
-- For simplicity of of converting it to the linq, let's assume here we only have one record and iterate on the C# side

-- once we get a result with 1+ records, we need to have to figure out what kind it is
	
	-- case 1: if RecordID is a purchase record
			select 
			spiritTypeRep.ProductTypeName [Spirit Short Name],
			spiritTypeRep.SpiritTypeReportingID
			from PurchaseToSpiritTypeReporting as pur2SpiritType 
			left join SpiritTypeReporting as spiritTypeRep on spiritTypeRep.SpiritTypeReportingID = pur2SpiritType.SpiritTypeReportingID
			where pur2SpiritType.PurchaseID = 499
			-- increment by Proof value from the record from the previous step
	
	-- case 2: if RecordID(s) is a production record
			select 
			spiritTypeRep.ProductTypeName [Spirit Short Name],
			spiritTypeRep.SpiritTypeReportingID
			from Production as prod 
			left join ProductionToSpiritTypeReporting as pur2SpiritType on pur2SpiritType.ProductionID = prod.ProductionID
			left join SpiritTypeReporting as spiritTypeRep on spiritTypeRep.SpiritTypeReportingID = pur2SpiritType.SpiritTypeReportingID
			where prod.ProductionID = 498 and prod.Gauged = 1
			-- increment by Proof value from the record from the previous step

       
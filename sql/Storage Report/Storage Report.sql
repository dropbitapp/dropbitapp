-- this query gets Proof that came from Purchases.
-- on the backend, the method name where it is implemented is called GetPurchasedStorageToProduction
select 
sourcePurchaseRecord.PurchaseID,
outputProductionRecord.ProductionID,
str.ProductTypeName [reportingCategoryName],
alcohol.Value [Alcohol],
productionContent.ContentValue,
productionContent.ContentFieldID,
(productionContent.ContentValue * alcohol.Value * 2) / 100 [Proof]

from Purchase as sourcePurchaseRecord 
left join Alcohol as alcohol on sourcePurchaseRecord.AlcoholID = alcohol.AlcoholID
left join ProductionContent as productionContent on sourcePurchaseRecord.PurchaseID = productionContent.RecordID
left join ContentField as contentField on productionContent.ContentFieldID = contentField.ContentFieldID 
left join Production as outputProductionRecord on productionContent.ProductionID = outputProductionRecord.ProductionID
left join ProductionType as productionType on outputProductionRecord.ProductionTypeID = productionType.ProductionTypeID
left join AspNetUserToDistiller as distiller on sourcePurchaseRecord.DistillerID = distiller.DistillerID
left join PurchaseToSpiritTypeReporting as p2str on sourcePurchaseRecord.PurchaseID = p2str.PurchaseID
left join SpiritTypeReporting as str on p2str.SpiritTypeReportingID = str.SpiritTypeReportingID 
where
distiller.UserId = 7 and
(sourcePurchaseRecord.PurchaseTypeID = 2 or
sourcePurchaseRecord.PurchaseTypeID = 3) and
outputProductionRecord.ProductionTypeID = 2 and
(sourcePurchaseRecord.StatusID = 1 or
sourcePurchaseRecord.StatusID = 2 or
sourcePurchaseRecord.StatusID = 3) and
sourcePurchaseRecord.PurchaseDate <= '09/01/2017'
and outputProductionRecord.ProductionEndTime >= '09/01/2017'
and outputProductionRecord.ProductionEndTime <= '09/30/2017'
and contentField.ContentFieldName ! = 'PurFermentedProofGal'
and contentField.ContentFieldName ! = 'PurDistilledProofGal'
and contentField.ContentFieldName ! = 'ProdDistilledProofGal'
and contentField.ContentFieldName ! = 'ProdBlendedProofGal'
and contentField.ContentFieldName ! = 'ProdFermentedProofGal'


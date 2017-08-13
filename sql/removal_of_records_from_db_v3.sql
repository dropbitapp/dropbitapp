-- this script removes the records associated with Production table and then all of the records associated with Purchase table

--Production--------------------------------------------------------------------
delete prod2Purch
from ProductionToPurchase as prod2Purch
inner join Production as prod
on prod2Purch.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete prod2SpirRepo
from ProductionToSpiritTypeReporting as prod2SpirRepo
inner join Production as prod
on prod2SpirRepo.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete vbw
from Weight as vbw
inner join Production as prod
on vbw.WeightID = prod.WeightID
where prod.DistillerID = 1

delete volume
from Volume as volume
inner join Production as prod
on volume.VolumeID = prod.VolumeID
where prod.DistillerID = 1

delete sto2Rec
from StorageToRecord as sto2Rec
inner join Production as prod
on sto2Rec.RecordId = prod.ProductionID
where sto2Rec.TableIdentifier like 'prod'and prod.DistillerID = 1

delete alc
from Alcohol as alc
inner join Production as prod
on alc.AlcoholID = prod.AlcoholID
where prod.DistillerID = 1

delete proof
from Proof as proof
inner join Production as prod
on proof.ProofID = prod.ProofID
where prod.DistillerID = 1

delete prod2Spirit
from ProductionToSpirit as prod2Spirit
inner join Production as prod
on prod2Spirit.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete prod2SpiritCut
from ProductionToSpiritCut as prod2SpiritCut
inner join Production as prod
on prod2SpiritCut.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete blendComponent
from BlendedComponent as blendComponent
inner join Production as prod
on blendComponent.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete bottlingInfo
from BottlingInfo as bottlingInfo
inner join Production as prod
on bottlingInfo.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete prod4Reporting
from Production4Reporting as prod4Reporting
inner join Production as prod
on prod4Reporting.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete prodCont
from ProductionContent as prodCont
inner join Production as prod
on prodCont.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete gainloss
from GainLoss as gainloss
inner join Production as prod
on gainloss.BlendedRecordId = prod.ProductionID
where prod.DistillerID = 1

delete stor2Rec
from StorageToRecord as stor2Rec
inner join Production as prod
on stor2Rec.RecordId = prod.ProductionID
where stor2Rec.TableIdentifier = 'prod' and prod.DistillerID = 1

delete blendComp
from BlendedComponent as blendComp
inner join Production as prod
on blendComp.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete blendComp
from BlendedComponent as blendComp
inner join Production as prod
on blendComp.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete blendCompHist
from BlendedComponentHist as blendCompHist
inner join Production as prod
on blendCompHist.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete prodH
from ProductionHist as prodH
inner join Production as prod
on prodH.ProductionID = prod.ProductionID
where prod.DistillerID = 1


delete taxW
from TaxWithdrawn as taxW
inner join Production as prod
on taxW.ProductionID = prod.ProductionID
where prod.DistillerID = 1

delete from Production
where DistillerID = 1


--Purchase--------------------------------------------------------------------
delete sto2Rec
from StorageToRecord as sto2Rec
inner join Purchase as purch
on sto2Rec.RecordId = purch.PurchaseID
where sto2Rec.TableIdentifier like 'pur' and purch.DistillerID = 1

delete prod2Purch
from ProductionToPurchase as prod2Purch
inner join Purchase as purch
on prod2Purch.PurchaseID = purch.PurchaseID
where purch.DistillerID = 1

delete alc
from Alcohol as alc
inner join Purchase as purch
on alc.AlcoholID = purch.AlcoholID
where purch.DistillerID = 1

delete proof
from Proof as proof
inner join Purchase as purch
on proof.ProofID = purch.ProofID
where purch.DistillerID = 1

delete vbw
from Weight as vbw
inner join Purchase as purch
on vbw.WeightID = purch.WeightID
where purch.DistillerID = 1

delete volume
from Volume as volume
inner join Purchase as purch
on volume.VolumeID = purch.VolumeID
where purch.DistillerID = 1

delete purch4Reporting
from Purchase4Reporting as purch4Reporting
inner join Purchase as purch
on purch4Reporting.PurchaseID = purch.PurchaseID
where purch.DistillerID = 1

delete stor2Rec
from StorageToRecord as stor2Rec
inner join Production as prod
on stor2Rec.RecordId = prod.ProductionID
where stor2Rec.TableIdentifier = 'pur' and prod.DistillerID = 1

delete purchHist
from PurchaseHist as purchHist
inner join Purchase as purch
on purchHist.PurchaseID = purch.PurchaseID
where purch.DistillerID = 1

delete purch4Rep
from Purchase4Reporting as purch4Rep
inner join Purchase as purch
on purch4Rep.PurchaseID = purch.PurchaseID
where purch.DistillerID = 1

delete from Purchase
where DistillerID = 1

-- checking tables for left over records
select * from BlendedComponent
select * from Alcohol
select * from BlendedComponentHist
select * from BottlingInfo
select * from BottlingInfoHist
select * from Destruction
select * from GainLoss
select * from Production
select * from Production4Reporting
select * from ProductionContent
select * from ProductionHist
select * from ProductionToPurchase
select * from ProductionToSpirit
select * from ProductionToSpiritCut
select * from ProductionToSpiritTypeReporting
select * from Proof
select * from Purchase
select * from Purchase4Reporting
select * from PurchaseHist
select * from Volume
select * from Weight
select * from TaxWithdrawn
select * from ContentField
select * from ProductionReportMaterialCategory
select * from ProdRepMatCat2MaterialKind
select * from AspNetUserToDistiller
select * from PurchaseToSpiritTypeReporting
select * from SpiritType2MaterialKindReporting
select * from MaterialKindReporting
select * from SpiritTypeReporting
select * from MaterialDict2MaterialCategory
select * from MaterialType
select * from PurchaseType
-- this script removes the records associated with Production table and then all of the records associated with Purchase table

--Production--------------------------------------------------------------------
delete prod2Purch
from ProductionToPurchase_v3 as prod2Purch
inner join Production_v3 as prod
on prod2Purch.ProductionID = prod.ProductionID

delete prod2SpirRepo
from ProductionToSpiritTypeReporting_v3 as prod2SpirRepo
inner join Production_v3 as prod
on prod2SpirRepo.ProductionID = prod.ProductionID

delete vbw
from VolumeByWeightLB_v3 as vbw
inner join Production_v3 as prod
on vbw.VolumeByWeightLBID = prod.VolumeByWeightLBID

delete volume
from QuantityGal_v3 as volume
inner join Production_v3 as prod
on volume.QuantityGalID = prod.QuantityGalID

delete sto2Rec
from StorageToRecord_v3 as sto2Rec
inner join Production_v3 as prod
on sto2Rec.RecordId = prod.ProductionID
where sto2Rec.TableIdentifier like 'prod'

delete alc
from Alcohol_v3 as alc
inner join Production_v3 as prod
on alc.AlcoholID = prod.AlcoholID

delete proof
from Proof_v3 as proof
inner join Production_v3 as prod
on proof.ProofID = prod.ProofID

delete prod2Spirit
from ProductionToSpirit_v3 as prod2Spirit
inner join Production_v3 as prod
on prod2Spirit.ProductionID = prod.ProductionID

delete prod2SpiritCut
from ProductionToSpiritCut_v3 as prod2SpiritCut
inner join Production_v3 as prod
on prod2SpiritCut.ProductionID = prod.ProductionID

delete blendComponent
from BlendedComponent_v3 as blendComponent
inner join Production_v3 as prod
on blendComponent.ProductionID = prod.ProductionID

delete bottlingInfo
from BottlingInfo_v3 as bottlingInfo
inner join Production_v3 as prod
on bottlingInfo.ProductionID = prod.ProductionID

delete prod4Reporting
from Production4Reporting_v3 as prod4Reporting
inner join Production_v3 as prod
on prod4Reporting.ProductionID = prod.ProductionID

delete from Production_v3


--Purchase_v3--------------------------------------------------------------------
delete sto2Rec
from StorageToRecord_v3 as sto2Rec
inner join Purchase_v3 as purch
on sto2Rec.RecordId = purch.PurchaseID
where sto2Rec.TableIdentifier like 'pur'

delete prod2Purch
from ProductionToPurchase_v3 as prod2Purch
inner join Purchase_v3 as purch
on prod2Purch.PurchaseID = purch.PurchaseID

delete alc
from Alcohol_v3 as alc
inner join Purchase_v3 as purch
on alc.AlcoholID = purch.AlcoholID

delete proof
from Proof_v3 as proof
inner join Purchase_v3 as purch
on proof.ProofID = purch.ProofID

delete vbw
from VolumeByWeightLB_v3 as vbw
inner join Purchase_v3 as purch
on vbw.VolumeByWeightLBID = purch.VolumeByWeightLBID

delete volume
from QuantityGal_v3 as volume
inner join Purchase_v3 as purch
on volume.QuantityGalID = purch.QuantityGalID


delete purch4Reporting
from Purchase4Reporting_v3 as purch4Reporting
inner join Purchase_v3 as purch
on purch4Reporting.PurchaseID = purch.PurchaseID

delete from Purchase_v3
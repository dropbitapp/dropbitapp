select 
prod.ProductionName
, prod.ProductionStartTime
, prod.ProductionEndTime
, prod.ProductioneDate
, prod.Note
, ISNULL(prod.ProductionID, 0) [ProductionID]
, prod.ProductionTypeID
, prodTypes.Name [ProdTypeName]
, ISNULL(galQuant.Value, 0) [Quantity]
, ISNULL(VBW.Value, 0) [VolumeByWeight]
, ISNULL(alc.Value, 0) [Alcohol]
, ISNULL(proof.Value, 0) [Proof]
, ISNULL(spiCuts.Name, '') [SpiritCut]
, ISNULL (spiCuts.SpiritCutID, 0) [SpiritCutID]
, ISNULL (spi.Name, '') [SpiritName]
, ISNULL (p2Spi.SpiritID, 0) [SpiritID]
from dbo.Production_v3 as prod
left join ProductionType_v3 as prodTypes on prod.ProductionTypeID = prodTypes.ProductionTypeID
left join dbo.QuantityGal_v3 as galQuant on prod.QuantityGalID = galQuant.QuantityGalID
left join dbo.VolumeByWeightLB_v3 as VBW on prod.VolumeByWeightLBID = VBW.VolumeByWeightLBID
left join dbo.Alcohol_v3 as alc on prod.AlcoholID = alc.AlcoholID
left join dbo.Proof_v3 as proof on prod.ProofID = proof.ProofID
left join dbo.ProductionToSpiritCut_v3 as spiCutsM on prod.ProductionID = spiCutsM.ProductionID
left join dbo.SpiritCut_v3 as spiCuts on spiCutsM.SpiritCutID = spiCuts.SpiritCutID
left join dbo.ProductionToSpirit_v3 as p2Spi on prod.ProductionID = p2Spi.ProductionID
left join dbo.Spirit_v3 as spi on p2Spi.SpiritID = spi.SpiritID
where  prodTypes.Name = 'Bottling'
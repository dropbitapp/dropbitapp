Select
prod.ProductionName [ProductionName],
status.Name [StatusName],
state.Name [StateName],
quants.Value [Quantity],
VBW.Value [VolumeByWeight],
alc.Value [Alcohol],
proof.Value [Proof],
spi.Name [SpiritName],
p2Spi.SpiritID [SpiritID]
from Production_v3 as prod 
left join QuantityGal_v3 as quants on prod.QuantityGalID = quants.QuantityGalID
left join VolumeByWeightLB_v3 as VBW on prod.VolumeByWeightLBID = VBW.VolumeByWeightLBID
left join Alcohol_v3 as alc on prod.AlcoholID = alc.AlcoholID
left join Proof_v3 as proof on prod.ProofID = proof.ProofID
left join ProductionToSpirit_v3 as p2Spi on prod.ProductionID = p2Spi.ProductionID
left join Spirit_v3 as spi on p2Spi.SpiritID = spi.SpiritID
left join Status_v3 as status on prod.StatusID = status.StatusID
left join State_v3 as state on prod.StateID = state.StateID
left join dbo.AspNetUsersToDistillers_v3 as distiller on prod.DistillerID = distiller.DistillerID
where (prod.StatusID = 1 or prod.StatusID = 2) and (prod.StateID = 4) and distiller.userID = UserId 
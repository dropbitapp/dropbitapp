-- get quantities for production record
  --select 
  --prod.ProductionID,
  --prod.ProductionName,
  --vol.Value [Volume],
  --wei.Value [Weight],
  --proof.Value [Proof]
  --from Production_v3 as prod
  --left join QuantityGal_v3 as vol on vol.QuantityGalID = prod.QuantityGalID
  --left join VolumeByWeightLB_v3 as wei on wei.VolumeByWeightLBID = prod.VolumeByWeightLBID
  --left join Proof_v3 as proof on proof.ProofID = prod.ProofID
  --where prod.ProductionID = 299
  
  -- get quantities for purchase record
  select
  pur.PurchaseID,
  pur.PurchaseName,
  vol.Value [Volume],
  wei.Value [Weight]
  from Purchase_v3 as pur
  left join QuantityGal_v3 as vol on vol.QuantityGalID = pur.QuantityGalID
  left join VolumeByWeightLB_v3 as wei on wei.VolumeByWeightLBID = pur.VolumeByWeightLBID
  where pur.PurchaseID = 1541

  --this displayes values from the [Production4Reporting_v3] and [Purchase4Reporting_v3]
  select 
  Proof
  from Production4Reporting_v3
  where ProductionID = 232

  --this displayes values from the [Purchase4Reporting_v3] 
  select 
  Proof
  from Purchase4Reporting_v3
  where PurchaseID = 1549
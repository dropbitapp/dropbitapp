 var res =
                from recs in db.Avail4Bottling_v3
                join prod in db.Production_v3 on recs.RecordId equals prod.ProductionID
                join prodTypes in db.ProductionType_v3 on prod.ProductionTypeID equals prodTypes.ProductionTypeID into prodTypes_join
                from prodTypes in prodTypes_join.DefaultIfEmpty()
                join galQuant in db.QuantityGal_v3 on prod.QuantityGalID equals galQuant.QuantityGalID into galQuant_join
                from galQuant in galQuant_join.DefaultIfEmpty()
                join VBW in db.VolumeByWeightLB_v3 on prod.VolumeByWeightLBID equals VBW.VolumeByWeightLBID into VBW_join
                from VBW in VBW_join.DefaultIfEmpty()
                join alc in db.Alcohol_v3 on prod.AlcoholID equals alc.AlcoholID into alc_join
                from alc in alc_join.DefaultIfEmpty()
                join proof in db.Proof_v3 on prod.ProofID equals proof.ProofID into proof_join
                from proof in proof_join.DefaultIfEmpty()
                join p2Spi in db.ProductionToSpirit_v3 on prod.ProductionID equals p2Spi.ProductionID into p2Spi_join
                from p2Spi in p2Spi_join.DefaultIfEmpty()
                join spi in db.Spirit_v3 on p2Spi.SpiritID equals spi.SpiritID into spi_join
                from spi in spi_join.DefaultIfEmpty()
                select new
                {
                    recs.TableIdentifier,
                    prod.ProductionName,
                    prod.Note,
                    ProductionID = ((System.Int32?)prod.ProductionID ?? (System.Int32?)0),
                    prod.ProductionTypeID,
                    ProdTypeName = prodTypes.Name,
                    Quantity = ((System.Single?)galQuant.Value ?? (System.Single?)0),
                    VolumeByWeight = ((System.Single?)VBW.Value ?? (System.Single?)0),
                    Alcohol = ((System.Single?)alc.Value ?? (System.Single?)0),
                    Proof = ((System.Single?)proof.Value ?? (System.Single?)0),
                    SpiritName = (spi.Name ?? ""),
                    SpiritID = ((System.Int32?)p2Spi.SpiritID ?? (System.Int32?)0)
                };
select Mats.MaterialDictID, mats.Name, mats.UnitOfMeasurementID, Mats.Note, Mats.UnitOfMeasurementID, units.Name
from dbo.MaterialDict_v3 as Mats
left join dbo.MaterialType_v3 as MatsType on Mats.MaterialDictID = MatsType.MaterialDictID
left join dbo.UnitOfMeasurement_v3 as units on Mats.UnitOfMeasurementID = units.UnitOfMeasurementID
where MatsType.Name = 'Fermentable'
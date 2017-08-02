select
material.MaterialDictID, material.Name,material.Note,
unit.UnitOfMeasurementID, unit.Name,
matType.MaterialDictID, matType.Name
from dbo.MaterialDict_v3 as material
left join dbo.UnitOfMeasurement_v3 as unit on material.UnitOfMeasurementID = unit.UnitOfMeasurementID
left join dbo.MaterialType_v3 as matType on material.MaterialDictID = matType.MaterialDictID

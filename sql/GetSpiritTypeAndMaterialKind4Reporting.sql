 Select
  spiType.SpiritTypeReportingID,
  spiType.ProductTypeName,
  spiT2Mat.MaterialKindReportingID,
  matKind.MaterialKindName
  
  from SpiritTypeReporting_v3 as spiType  
  left join SpiritType2MaterialKindReporting as spiT2Mat on spiT2Mat.SpiritTypeReportingID = spiType.SpiritTypeReportingID
  left join MaterialKindReporting_v3 matKind on spiT2Mat.MaterialKindReportingID = matKind.MaterialKindReportingID

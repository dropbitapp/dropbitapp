select *
from dbo.StorageToRecord_v3 as rec
left join dbo.Storage_v3  as stoName on rec.StorageID = stoName.StorageID
where rec.RecordId = 6 AND rec.TableIdentifier = 'pur'
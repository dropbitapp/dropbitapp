/*
	move storage data
*/

--insert into dbo.Storage_v3( 
--Name
--,Capacity
--,SerialNumber
--,Note
--,DistillerID
--)
--select 
--StorageName
--,0
--,SerialNumber
--,Note 
--,0
--from dbo.Storage

/*
	move spirit name data
*/

--insert into dbo.Spirit_v3
--(
--DistillerID
--,Name
--,Note
--)
--select
--0
--,SpiritName
--,Note
--from dbo.Spirit

/*
	move vendor name data
*/

--insert into dbo.Vendor_v3
--(
--Name,
--DistillerID
--)
--select 
--VendorName
--,0
--from dbo.Vendor

/*
	Move Units Of Measurement
*/

--insert into dbo.UnitOfMeasurement_v3
--(
--Name
--)
--select 
--UnitName
--from dbo.UnitOfMeasurement

/*
	move Materials (this only moves materials but not any meta data assoictaed with it
*/

--insert into dbo.MaterialDict_v3
--(
--	Name
--	,UnitOfMeasurementID
--	,Note
--	,DistillerID
--)

--select 
--RawMaterialName
--,0
--,Note
--,0
--  FROM dbo.RawMaterial

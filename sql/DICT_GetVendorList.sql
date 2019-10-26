/*
	used in Dictionary Vendor Ciew workflow to display all vendor records
*/

select 
vendRes.VendorID,
vendRes.Name,
ISNULL(vendDetails.Note, '') [Note]
from dbo.Vendor_v3 as vendRes
left join dbo.VendorDetail_v3 as vendDetails on vendRes.VendorID = vendDetails.VendorID

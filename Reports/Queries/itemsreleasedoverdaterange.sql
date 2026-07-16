IF (OBJECT_ID ('tempdb..#TMP_validMSDS')  is not null)
BEGIN
Drop table #TMP_validMSDS
END
IF (OBJECT_ID ('tempdb..#TMP_validMSDS')  is  null)
BEGIN
SELECT * INTO #TMP_validMSDS
FROM (
select p21_view_inventory_supplier.inv_mast_uid,
p21_view_inventory_supplier.msds,
isdate(p21_view_inventory_supplier.msds) as validDate,
CASE
WHEN msds LIKE '[1-2][0-9][0-9][0-9]/[0-1][0-9]/[0-3][0-9]' THEN  CONVERT(datetime,msds,111)
WHEN msds LIKE '[1-2][0-9][0-9][0-9]/[0-1][0-9]/[0][0-3][0-9]' THEN  CONVERT(datetime,msds,111)
WHEN msds LIKE '[1-2][0-9][0-9][0-9]/[0-9]/[0-3][0-9]' THEN  CONVERT(datetime,msds,111)
WHEN msds LIKE '[1-2][0-9][0-9][0-9]/[0-1][0-9]/[0-9]' THEN  CONVERT(datetime,msds,111)
WHEN msds LIKE '[0-1][0-9]/[0-3][0-9]/[1-2][0-9][0-9][0-9]' THEN  CONVERT(datetime,msds,111)
WHEN msds LIKE '[1-2][0-9][0-9][0-9]-[0-1][0-9]-[0-3][0-9]'THEN  CONVERT(datetime,msds,111)
WHEN msds LIKE '[0-1][0-9]-[0-3][0-9]-[1-2][0-9][0-9][0-9]' THEN  CONVERT(datetime,msds,111)
WHEN msds LIKE '[0-9]/[0-3][0-9]/[1-2][0-9][0-9][0-9]' THEN  CONVERT(datetime,msds,111)
WHEN msds LIKE '[0-9]/[0-9]/[1-2][0-9][0-9][0-9]' THEN  CONVERT(datetime,msds,111)
WHEN msds LIKE '[0-1][0-9]/[0-9]/[1-2][0-9][0-9][0-9]' THEN  CONVERT(datetime,msds,111)
ELSE null
END as MSDS_DATE
from p21_view_inventory_supplier where
isdate(p21_view_inventory_supplier.msds)=1
) t
END
IF (OBJECT_ID ('tempdb..#TMP_validMSDS')  is not null)
BEGIN
SELECT  distinct p21_view_inv_mast.item_id 
,p21_view_inv_mast.item_desc 
,COALESCE(ud.release_date,msds.MSDS_DATE) as Release_Date
FROM p21_view_inv_loc 
JOIN p21_view_inv_mast ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid 
JOIN p21_view_inventory_supplier ON p21_view_inventory_supplier.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
join inv_mast_ud ud on ud.inv_mast_uid = p21_view_inv_mast.inv_mast_uid 
left join #TMP_validMSDS msds on  msds.inv_mast_uid =p21_view_inventory_supplier.inv_mast_uid   
AND ISDATE(msds.MSDS_DATE) = 1 
WHERE COALESCE(ud.release_date,msds.MSDS_DATE) IS NOT NULL 
AND COALESCE(ud.release_date, 
CASE WHEN ISDATE(msds.MSDS_DATE) = 1 THEN CAST(msds.MSDS_DATE AS DATETIME) ELSE '1920-01-01 00:00:00.000' END
) BETWEEN {dateRange} 
AND p21_view_inv_loc.location_id =@locationId 
AND p21_view_inv_loc.qty_on_hand > $0 
AND (p21_view_inv_loc.delete_flag = 'N' OR p21_view_inv_loc.delete_flag IS null) 
ORDER BY 3, p21_view_inv_mast.item_id 
END ;
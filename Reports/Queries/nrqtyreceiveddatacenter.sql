IF (OBJECT_ID ('tempdb..#TMP_inv_supplv1')  is not null)
BEGIN
Drop table #TMP_inv_supplv1
END
IF (OBJECT_ID ('tempdb..#TMP_inv_supplv1')  is  null)
BEGIN
SELECT * INTO  #TMP_inv_supplv1
FROM (
select p21_view_inventory_supplier.inv_mast_uid,
supplier_id,
division_id,
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
from p21_view_inventory_supplier  where
isdate(p21_view_inventory_supplier.msds)=1
 ) t
END
IF (OBJECT_ID ('tempdb..#TMP_inv_supplv1')  is not null)
BEGIN
SELECT s.msds,
 mast.item_id,
 mast.item_desc,
 loc.price1,
 CAST(line.qty_received AS INT ) as qty_received
 FROM p21_view_inv_loc loc  
 inner join p21_view_inv_mast mast with (NOLOCK) on loc.item_id =mast.item_id
 inner join p21_view_inventory_supplier s  with (NOLOCK) on mast.item_id =s.item_id 
 inner join p21_view_po_line line with (NOLOCK) on line.item_id = s.item_id 
 left join #TMP_inv_supplv1 v with (NOLOCK) on s.inv_mast_uid=v.inv_mast_uid 
 and s.supplier_id=v.supplier_id 
 and s.division_id=v.division_id 
WHERE loc.location_id=@locationId 
and s.msds is not null
AND v.MSDS_DATE BETWEEN {dateRange}
END
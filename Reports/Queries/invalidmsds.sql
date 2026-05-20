IF (OBJECT_ID ('tempdb..#TMP_invalidMSDS')  is not null)
BEGIN
Drop table #TMP_invalidMSDS
END
IF (OBJECT_ID ('tempdb..#TMP_invalidMSDS')  is  null)
BEGIN
SELECT * INTO #TMP_invalidMSDS
FROM (
select p21_view_inventory_supplier.inv_mast_uid,
supplier_id,
division_id,
p21_view_inventory_supplier.msds,
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
isdate(p21_view_inventory_supplier.msds)=0
) t
END
select s.item_id, 
inv.item_desc, 
s.supplier_id, 
s.division_id,
inv.date_created, 
s.msds
from p21_view_inventory_supplier s
inner join #TMP_invalidMSDS v  on s.inv_mast_uid=v.inv_mast_uid 
and s.supplier_id=v.supplier_id and s.division_id=v.division_id 
left join inv_mast inv on inv.inv_mast_uid=s.inv_mast_uid  
and inv.item_id = s.item_id 
where s.delete_flag ='N' 
and  s.msds is not null
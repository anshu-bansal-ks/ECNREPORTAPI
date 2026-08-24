SELECT p21_view_inv_mast.item_id
,p21_view_inv_mast.item_desc 
{Qtydisstats}
,p21_view_inv_loc.primary_bin
FROM p21_view_inv_mast(NOLOCK)
JOIN p21_view_inv_loc(NOLOCK)
ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
JOIN V_QTY(NOLOCK)
ON p21_view_inv_mast.inv_mast_uid = V_QTY.UID
WHERE ((p21_view_inv_mast.item_desc LIKE '%(disc)%')
AND (V_QTY.tot_Qty = $0)
AND (p21_view_inv_loc.location_id = @locationId)
AND (p21_view_inv_mast.delete_flag = 'n') )
AND p21_view_inv_loc.primary_bin >'1'
ORDER BY p21_view_inv_mast.item_id
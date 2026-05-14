SELECT  p21_view_inv_mast.item_id 
,p21_view_inv_mast.item_desc
, CAST(p21_view_inv_loc.qty_on_hand AS INT) as qty_on_hand
,p21_view_inv_loc.primary_bin 
FROM p21_view_inv_mast (NOLOCK)
JOIN p21_view_inv_loc (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid 
JOIN p21_view_inventory_supplier (NOLOCK) ON p21_view_inventory_supplier.inv_mast_uid = p21_view_inv_loc.inv_mast_uid 
WHERE   p21_view_inv_mast.delete_flag = 'n' 
AND p21_view_inv_mast.item_desc NOT LIKE 'no long%' 
AND p21_view_inventory_supplier.supplier_id = @supplierId 
AND p21_view_inventory_supplier.delete_flag = 'n' 
AND p21_view_inv_loc.location_id = @locationId 
ORDER BY p21_view_inv_loc.primary_bin
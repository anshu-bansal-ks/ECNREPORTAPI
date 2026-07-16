SELECT p21_view_inv_mast.item_id
,p21_view_inv_mast.item_desc
,l.location_name
,p21_view_inv_loc.primary_bin
,CAST(p21_view_inv_loc.qty_on_hand as INT) as qty_on_hand
,pg.product_group_desc
,ud.expiration_date
FROM p21_view_inv_mast
JOIN p21_view_inv_loc (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
JOIN p21_view_inventory_supplier (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inventory_supplier.inv_mast_uid
JOIN dbo.product_group AS pg ( NOLOCK ) ON pg.product_group_id = p21_view_inv_loc.product_group_id
JOIN location (NOLOCK) l ON l.location_id = p21_view_inv_loc.location_id
left join inv_loc_ud (NOLOCK) ud on ud.location_id = l.location_id and ud.inv_mast_uid =  p21_view_inv_mast.inv_mast_uid
WHERE p21_view_inventory_supplier.delete_flag = 'n'
AND p21_view_inv_mast.item_desc NOT LIKE 'NO LONGER AVAILABLE%'
AND p21_view_inv_loc.location_id = @locationId
AND p21_view_inv_mast.item_desc NOT LIKE '%tester%'
AND p21_view_inv_loc.primary_bin NOT IN ('0','00')
AND (@stockable = 'True' OR (ud.expiration_date IS NOT NULL AND ud.expiration_date != ''))
Order by p21_view_inv_mast.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
SELECT im.item_id
,invsup.supplier_part_no
,im.item_desc
,im.price1
,invsup.cost
,il.qty_on_hand
,ISNULL(im.class_id4, '') as class_id4
,ISNULL(v_upc.upc, '') as upc
FROM p21_view_inv_loc (NOLOCK) il
JOIN p21_view_inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN p21_view_inventory_supplier (NOLOCK) invsup ON invsup.inv_mast_uid = il.inv_mast_uid
JOIN dbo.inventory_supplier_x_loc (NOLOCK) AS isxl ON (isxl.inventory_supplier_uid = invsup.inventory_supplier_uid 
AND isxl.location_id = @locationId)
LEFT outer JOIN inv_mast_ud (NOLOCK) ud ON ud.inv_mast_uid = im.inv_mast_uid
LEFT OUTER JOIN v_upc ON v_upc.inv_mast_uid = im.inv_mast_uid
WHERE il.location_id = @locationId
AND il.qty_on_hand >= $0
AND im.item_desc NOT LIKE 'no longer%'
AND invsup.supplier_id = @supplierId
AND (im.delete_flag not in ('Y', '3') or im.delete_flag is null)
AND invsup.delete_flag = 'n'
AND isxl.primary_supplier = 'y'
order by im.item_id
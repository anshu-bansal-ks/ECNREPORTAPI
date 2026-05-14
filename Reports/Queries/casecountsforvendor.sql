SELECT im.item_id
, ISNULL(v_upc.upc, '') as upc
, im.item_desc
, iu.unit_of_measure
, uom.unit_description
, CAST(iu.unit_size AS INT) as unit_size
FROM item_uom iu
JOIN p21_view_inv_mast im ON im.inv_mast_uid = iu.inv_mast_uid
JOIN dbo.p21_view_unit_of_measure uom ON uom.unit_id = iu.unit_of_measure
JOIN dbo.p21_view_inventory_supplier invsup ON invsup.inv_mast_uid = im.inv_mast_uid
JOIN dbo.p21_view_inventory_supplier_x_loc supploc ON supploc.inventory_supplier_uid = invsup.inventory_supplier_uid
AND supploc.location_id = @locationId
AND supploc.primary_supplier = 'Y'
LEFT OUTER JOIN dbo.v_upc ON v_upc.inv_mast_uid = im.inv_mast_uid
WHERE iu.unit_of_measure = 'CS'
AND invsup.supplier_id = @supplierId
AND im.delete_flag = 'N'
AND iu.delete_flag = 'N'
ORDER BY im.item_id
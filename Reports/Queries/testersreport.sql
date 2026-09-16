SELECT im.item_id
, im.item_desc
, ivsupp.supplier_id
, s.supplier_name
{QtytesSubQuery}
, CAST(q.Tot_Qty AS INT) as total
FROM p21_view_inv_mast im
JOIN p21_view_inv_loc il ON il.inv_mast_uid = im.inv_mast_uid
JOIN dbo.p21_view_inventory_supplier ivsupp ON ivsupp.inv_mast_uid = im.inv_mast_uid
JOIN dbo.p21_view_inventory_supplier_x_loc suploc ON suploc.inventory_supplier_uid = ivsupp.inventory_supplier_uid
AND suploc.location_id = il.location_id
JOIN p21_view_supplier s ON s.supplier_id = ivsupp.supplier_id
JOIN V_QTY q ON q.UID = im.inv_mast_uid
WHERE il.location_id = @loctionid
AND im.price1 = 0
AND suploc.primary_supplier = 'Y'
AND il.standard_cost = 0
AND im.delete_flag = 'N'
AND im.other_charge_item = 'N'
AND il.discontinued = 'N'
ORDER BY supplier_name
, im.item_id
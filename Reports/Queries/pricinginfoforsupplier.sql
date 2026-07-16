SELECT p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, price8 as MSRP
, price7 as MAP
, price1
, cost
FROM p21_view_inv_mast
JOIN p21_view_inventory_supplier ON p21_view_inventory_supplier.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
WHERE p21_view_inv_mast.delete_flag = 'n'
AND p21_view_inventory_supplier.supplier_id = @supplierId
AND p21_view_inv_mast.delete_flag = 'N'
AND p21_view_inventory_supplier.delete_flag = 'n'
ORDER BY p21_view_inv_mast.item_id
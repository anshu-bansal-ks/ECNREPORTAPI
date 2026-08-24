SELECT p21_view_inv_mast.item_id  
, p21_view_inv_mast.item_desc  
 {Qtystats}
, CAST(V_QTY.Tot_Qty  AS INT) as total
, p21_view_inv_mast.price1 
, p21_view_inventory_supplier.cost
FROM p21_view_inv_mast (NOLOCK)
JOIN V_QTY (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = V_QTY.UID
JOIN p21_view_inventory_supplier (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inventory_supplier.inv_mast_uid
JOIN dbo.p21_view_supplier (NOLOCK) ON p21_view_supplier.supplier_id = p21_view_inventory_supplier.supplier_id
WHERE p21_view_inventory_supplier.delete_flag = 'n'
AND p21_view_inv_mast.delete_flag = 'n'
AND (@status <> 'true' OR V_QTY.Tot_Qty > 0)
AND p21_view_inventory_supplier.supplier_id = @supplierId
ORDER BY p21_view_inv_mast.item_id

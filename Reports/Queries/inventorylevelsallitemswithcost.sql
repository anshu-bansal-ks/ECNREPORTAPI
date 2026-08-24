SELECT p21_view_inventory_supplier.supplier_id
,p21_view_supplier.supplier_name
,p21_view_inv_mast.item_id
,p21_view_inv_mast.item_desc
{QtySubQuery}
,CAST(V_QTY.Tot_Qty AS INT) as total
,p21_view_inv_mast.price1
,p21_view_inventory_supplier.cost as supplier_cost
FROM p21_view_inv_mast (NOLOCK)
JOIN V_QTY (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = V_QTY.UID
JOIN p21_view_inventory_supplier (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inventory_supplier.inv_mast_uid
JOIN dbo.p21_view_supplier (NOLOCK) ON p21_view_supplier.supplier_id = p21_view_inventory_supplier.supplier_id
WHERE p21_view_inventory_supplier.delete_flag = 'n'
AND p21_view_inv_mast.delete_flag = 'n'
AND (@status <> 'false' OR V_QTY.Tot_Qty > 0)
ORDER BY p21_view_inv_mast.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
SELECT p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, CAST(V_QTY.NJ_QTY AS INT ) as NJ_QTY
, CAST(V_QTY.FL_Qty AS INT ) as FL_Qty
, CAST(V_QTY.CA_Qty AS INT ) as CA_Qty
, CAST(V_QTY.Tot_Qty AS INT ) as total_qty
, p21_view_inventory_supplier.msds
, dbo.inv_mast_ud.release_date
, p21_view_inv_mast.price9
, v_upc.upc
, p21_view_inv_mast.price1
FROM p21_view_inv_mast
JOIN p21_view_inventory_supplier (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inventory_supplier.inv_mast_uid
JOIN V_QTY (NOLOCK) ON p21_view_inventory_supplier.inv_mast_uid = V_QTY.UID
JOIN inv_mast_ud (NOLOCK) ON inv_mast_ud.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
LEFT JOIN v_upc (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = v_upc.inv_mast_uid
WHERE (( V_QTY.Tot_Qty > $0 )
AND inv_mast_ud.release_date > CAST(@releasedate AS DATE)
AND ( p21_view_inv_mast.price9 <> $5.000000000 )
and (p21_view_inventory_supplier.delete_flag = 'n'));
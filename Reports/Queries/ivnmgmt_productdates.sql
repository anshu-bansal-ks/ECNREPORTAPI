SELECT p21_view_inv_mast.item_id
 , p21_view_inv_mast.item_desc
 , p21_view_inventory_supplier.msds
 , ud.release_date
 , p21_view_inventory_supplier.supplier_id
 , p21_view_inv_loc.buy as buyable
 , p21_view_inv_loc.stockable
 , p21_view_inv_loc.last_purchase_date
 , p21_view_inv_mast.date_created
 , CAST(V_QTY.NJ_QTY AS INT) as NJ_QTY
 , CAST(V_QTY.FL_Qty AS INT) as FL_Qty
 , CAST(V_QTY.CA_Qty AS INT) as CA_Qty
 FROM p21_view_inv_loc
 JOIN p21_view_inv_mast ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
 JOIN p21_view_inventory_supplier ON p21_view_inventory_supplier.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
 JOIN V_QTY ON V_QTY.UID = p21_view_inv_mast.inv_mast_uid
 LEFT OUTER JOIN inv_mast_ud (NOLOCK) ud ON ud.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
 WHERE ud.release_date >= CAST(@releasedate AS DATE)
 AND p21_view_inv_loc.location_id = '100035'
 AND p21_view_inventory_supplier.delete_flag = 'n'
 AND p21_view_inv_mast.delete_flag = 'n'
 AND p21_view_inv_mast.item_id NOT LIKE 'ADS%'
 AND p21_view_inv_mast.item_desc NOT LIKE '%TESTER%'
 ORDER BY item_id
SELECT p21_view_inventory_supplier.supplier_id
, p21_view_supplier.supplier_name
, p21_view_division.division_name
, p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, CAST(p21_view_inv_loc.qty_on_hand AS INT) as qty_on_hand
, CAST(p21_view_inv_loc.qty_allocated AS INT) as qty_allocated
, p21_view_inv_loc.moving_average_cost
, (p21_view_inv_loc.qty_on_hand * p21_view_inv_loc.moving_average_cost) as total
FROM p21_view_division p21_view_division
, p21_view_inv_loc p21_view_inv_loc
, p21_view_inv_mast p21_view_inv_mast
, p21_view_inventory_supplier p21_view_inventory_supplier
, p21_view_supplier p21_view_supplier
WHERE p21_view_inv_mast.inv_mast_uid = p21_view_inventory_supplier.inv_mast_uid
AND p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
AND p21_view_inventory_supplier.supplier_id = p21_view_supplier.supplier_id
AND p21_view_inventory_supplier.division_id = p21_view_division.division_id
AND p21_view_inv_loc.location_id=@locationId 
AND p21_view_inventory_supplier.delete_flag='n'
AND p21_view_inv_mast.delete_flag='n'
AND p21_view_inv_loc.qty_on_hand>$0
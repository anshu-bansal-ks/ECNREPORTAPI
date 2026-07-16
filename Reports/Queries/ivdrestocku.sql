SELECT p21_view_inventory_supplier.supplier_id,
p21_view_supplier.supplier_name, 
p21_view_inv_mast.item_id,
p21_view_inv_mast.item_desc,
p21_view_inventory_supplier.msds,
CAST(p21_view_po_line.qty_ordered AS INT) as qty_ordered,
CAST(p21_view_po_line.qty_received AS INT) as qty_received,
CAST(p21_view_inv_loc.qty_on_hand AS INT) as qty_on_hand,
CAST(p21_view_inv_loc.qty_in_transit AS INT) as qty_in_transit,
CAST(p21_view_inv_loc.order_quantity AS INT) as order_quantity,
CAST(p21_view_inv_loc.qty_allocated AS INT) as qty_allocated,
CAST(p21_view_inv_loc.qty_backordered AS INT) as qty_backordered,
p21_view_po_line.po_no,
p21_view_supplier.buyer_id
FROM p21_view_inv_loc p21_view_inv_loc,
p21_view_inv_mast p21_view_inv_mast,
p21_view_inventory_supplier p21_view_inventory_supplier,
p21_view_po_hdr p21_view_po_hdr, p21_view_po_line p21_view_po_line,
p21_view_supplier p21_view_supplier
WHERE p21_view_inventory_supplier.supplier_id = p21_view_supplier.supplier_id
AND p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
AND p21_view_inv_loc.inv_mast_uid = p21_view_inventory_supplier.inv_mast_uid
AND p21_view_po_line.inv_mast_uid = p21_view_inventory_supplier.inv_mast_uid
AND p21_view_po_line.po_no = p21_view_po_hdr.po_no 
AND ((p21_view_supplier.buyer_id='1290')
AND (p21_view_inv_loc.location_id= @locationId) 
AND (p21_view_inv_loc.sellable='y')
AND (p21_view_inv_loc.product_group_id='nrdvd')
AND (p21_view_inv_loc.qty_on_hand<=(p21_view_po_line.qty_received/$2))
AND (p21_view_inventory_supplier.delete_flag='n')
AND p21_view_po_hdr.location_id= @locationId
AND (p21_view_inv_mast.delete_flag='n'))
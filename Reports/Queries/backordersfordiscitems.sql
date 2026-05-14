SELECT p21_view_oe_hdr.customer_id
, p21_view_oe_hdr.ship2_name
, p21_view_oe_line.order_no
, p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, p21_view_oe_line.qty_ordered
, p21_view_oe_line.qty_on_pick_tickets
, p21_view_oe_line.disposition
, p21_view_oe_line.qty_invoiced
, p21_view_inv_loc.qty_on_hand
, p21_view_inv_loc.qty_allocated
FROM p21_view_customer p21_view_customer
, p21_view_inv_loc p21_view_inv_loc
, p21_view_inv_mast p21_view_inv_mast
, p21_view_inventory_supplier p21_view_inventory_supplier
, p21_view_oe_hdr p21_view_oe_hdr
, p21_view_oe_hdr_salesrep p21_view_oe_hdr_salesrep
, p21_view_oe_line p21_view_oe_line
WHERE p21_view_oe_hdr.customer_id = p21_view_customer.customer_id 
AND p21_view_oe_hdr.company_id = p21_view_customer.company_id 
AND p21_view_oe_hdr_salesrep.salesrep_id = p21_view_customer.salesrep_id 
AND p21_view_oe_line.inv_mast_uid = p21_view_inv_mast.inv_mast_uid 
AND p21_view_oe_hdr.order_no = p21_view_oe_hdr_salesrep.order_number 
AND p21_view_oe_hdr.order_no = p21_view_oe_line.order_no 
AND p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid 
AND p21_view_oe_line.inv_mast_uid = p21_view_inventory_supplier.inv_mast_uid 
AND ((p21_view_oe_hdr.order_date>={ts '2009-06-01 00:00:00'}) 
AND (p21_view_oe_line.complete='N') 
AND (p21_view_oe_hdr.rma_flag='N') 
AND (p21_view_oe_hdr.projected_order='N')
AND (p21_view_oe_line.disposition='b')  
AND (p21_view_inv_loc.location_id=@locationId) 
AND (p21_view_oe_hdr.source_location_id=@locationId) 
AND (p21_view_inventory_supplier.supplier_id=@supplierId))
ORDER BY p21_view_inv_mast.item_id
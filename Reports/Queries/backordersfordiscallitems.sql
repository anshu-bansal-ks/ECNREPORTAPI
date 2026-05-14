SELECT p21_view_oe_line.order_no
, p21_view_oe_hdr.customer_id
, p21_view_oe_hdr.ship2_name
, p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
,  CAST(p21_view_oe_line.qty_ordered AS INT) AS qty_ordered
, CAST(p21_view_oe_line.qty_on_pick_tickets AS INT) AS qty_on_pick_tickets
, p21_view_oe_line.disposition 
, CAST(p21_view_oe_line.qty_invoiced AS INT) AS qty_invoiced
, CAST(p21_view_inv_loc.qty_on_hand AS INT) AS qty_on_hand
, CAST(p21_view_inv_loc.qty_allocated AS INT) AS qty_allocated
, p21_view_oe_hdr_salesrep.date_created
FROM p21_view_oe_hdr
JOIN p21_view_oe_line (NOLOCK) ON p21_view_oe_hdr.order_no = p21_view_oe_line.order_no
JOIN p21_view_oe_hdr_salesrep (NOLOCK) ON p21_view_oe_hdr.order_no = p21_view_oe_hdr_salesrep.order_number
JOIN p21_view_inv_mast (NOLOCK) ON p21_view_oe_line.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
JOIN p21_view_inv_loc (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
JOIN p21_view_customer (NOLOCK) ON p21_view_oe_hdr.customer_id = p21_view_customer.customer_id
WHERE(( p21_view_oe_hdr.order_date >= {TS '2014-06-01 00:00:00'} )
AND ( p21_view_oe_line.complete = 'N' )
AND ( p21_view_oe_hdr.rma_flag = 'N' )
AND ( p21_view_oe_hdr.projected_order = 'N' )
AND ( p21_view_oe_line.disposition = 'b' )
AND ( p21_view_inv_loc.location_id = @locationId )
AND ( p21_view_oe_hdr.source_location_id = @locationId )
AND ( p21_view_inv_mast.item_desc LIKE '%(disc)%' )
AND ( p21_view_inv_loc.qty_on_hand = $.000000000 ))
ORDER BY p21_view_oe_hdr_salesrep.date_created
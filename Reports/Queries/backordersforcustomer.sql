SELECT inv_mast.item_id
,inv_mast.item_desc
,address.phys_state as source
,oe_hdr.order_date
,oe_hdr.order_no
,oe_hdr.po_no
,( oe_line.qty_ordered - oe_line.qty_on_pick_tickets- oe_line.qty_invoiced - oe_line.qty_canceled ) AS 'qty_bo'
{QtyColumns}
FROM oe_hdr WITH ( NOLOCK )
JOIN oe_hdr_salesrep (NOLOCK) ON oe_hdr.order_no = oe_hdr_salesrep.order_number
JOIN oe_line (NOLOCK) ON oe_hdr.order_no = oe_line.order_no
JOIN inv_mast WITH ( NOLOCK ) ON inv_mast.inv_mast_uid = oe_line.inv_mast_uid
JOIN inv_loc WITH ( NOLOCK ) ON inv_loc.inv_mast_uid = inv_mast.inv_mast_uid
LEFT OUTER JOIN v_qty (NOLOCK) da_invqty ON DA_INVQTY.UID = inv_mast.inv_mast_uid
JOIN address(NOLOCK) ON address.id = oe_hdr.source_location_id
WHERE oe_hdr.customer_id = @custId
AND oe_hdr.delete_flag = 'N'
AND oe_line.delete_flag = 'N'
AND oe_line.disposition IN ( 'B' )
AND inv_loc.location_id = oe_hdr.source_location_id
AND oe_hdr.rma_flag = 'N'
AND oe_hdr.projected_order = 'N'
AND oe_hdr.cancel_flag = 'N'
AND oe_hdr.completed ='N'
ORDER BY oe_hdr.order_no 
,inv_mast.item_id
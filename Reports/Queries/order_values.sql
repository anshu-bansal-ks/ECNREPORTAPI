SELECT p21_view_oe_hdr.order_date
, p21_view_oe_hdr.order_no
, p21_view_oe_hdr.customer_id
, p21_view_oe_hdr.ship2_name
, p21_view_oe_line.item_id
, p21_view_inv_mast.item_desc
, ISNULL(p21_view_oe_line.disposition, '') as disposition
, p21_view_oe_line.delete_flag 
, CAST(p21_view_oe_line.qty_ordered AS INT) as qty_ordered
, CAST((qty_on_pick_tickets + qty_allocated + qty_invoiced) AS INT) as qty_filled
, ROUND((qty_on_pick_tickets + qty_allocated + qty_invoiced) * unit_price, 2) as Amt_Filled
, CAST(p21_view_oe_line.qty_canceled AS INT) as qty_canceled
, ROUND(p21_view_oe_line.qty_canceled * unit_price, 2) as Amt_Canceled
, CAST((p21_view_oe_line.qty_ordered - qty_invoiced - qty_on_pick_tickets - qty_canceled - qty_allocated) AS INT) as qty_backordered
, ROUND( (p21_view_oe_line.qty_ordered - qty_invoiced - qty_on_pick_tickets - qty_canceled - qty_allocated) * unit_price , 2) as Amt_backordered
FROM p21_view_oe_hdr (NOLOCK)
JOIN p21_view_oe_line (NOLOCK) ON p21_view_oe_line.order_no = p21_view_oe_hdr.order_no
JOIN p21_view_inv_mast (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_oe_line.inv_mast_uid
WHERE p21_view_oe_hdr.delete_flag = 'N'
AND p21_view_oe_line.delete_flag = 'N'
AND p21_view_oe_hdr.order_no = @ordernum
ORDER BY p21_view_oe_line.disposition desc
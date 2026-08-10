SELECT oe_hdr.customer_id
,oe_hdr.ship2_name
,DA_Rep.rep
,oe_hdr.order_no
,oe_hdr.po_no
,SUM(( oe_line.qty_ordered - oe_line.qty_canceled - oe_line.qty_on_pick_tickets-oe_line.qty_invoiced )
/ oe_line.pricing_unit_size * oe_line.unit_price) as sales
FROM oe_hdr
JOIN oe_line (NOLOCK) ON oe_hdr.order_no = oe_line.order_no
LEFT OUTER JOIN DA_Rep (NOLOCK) ON oe_hdr.customer_id = DA_Rep.customer_id
JOIN supplier (NOLOCK) ON oe_line.supplier_id = supplier.supplier_id
WHERE (po_no LIKE '%' +@pono+ '%' or po_no like +@pono+ '%') 
AND oe_hdr.order_date BETWEEN {dateRange}
AND oe_hdr.cancel_flag = 'N'
AND oe_line.cancel_flag = 'N'
AND oe_line.delete_flag = 'N'
AND oe_hdr.delete_flag = 'N'
AND oe_hdr.completed = 'N'
AND oe_hdr.projected_order = 'N'
and oe_hdr.approved = 'Y'
AND oe_line.disposition = 'B'
GROUP BY oe_hdr.customer_id
,oe_hdr.ship2_name
,oe_hdr.order_no
,oe_hdr.po_no
,DA_Rep.rep
ORDER BY rep, ship2_name
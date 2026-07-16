SELECT p21_view_oe_pick_ticket.location_id
,p21_view_location.location_name
,CAST((SUM(p21_view_invoice_line.qty_shipped)) AS INT) as pieces_shipped
,SUM(p21_view_invoice_line.extended_price) as sales
FROM p21_view_invoice_hdr (NOLOCK)
JOIN p21_view_invoice_line (NOLOCK) ON p21_view_invoice_hdr.invoice_no = p21_view_invoice_line.invoice_no
JOIN dbo.p21_view_oe_pick_ticket (NOLOCK) ON p21_view_invoice_hdr.invoice_no = p21_view_oe_pick_ticket.invoice_no
JOIN p21_view_location (NOLOCK) ON p21_view_oe_pick_ticket.location_id = p21_view_location.location_id
WHERE dbo.p21_view_invoice_hdr.invoice_date BETWEEN {dateRange}
AND p21_view_invoice_line.item_id <> 'freight out'
AND p21_view_invoice_line.item_id <> 'delivery'
AND p21_view_invoice_line.item_id <> 'downpayment'
AND (p21_view_invoice_line.extended_price) > 0
GROUP BY p21_view_oe_pick_ticket.location_id
,p21_view_location.location_name
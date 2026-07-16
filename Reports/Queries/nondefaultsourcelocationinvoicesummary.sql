SELECT ih.invoice_no
, ih.order_no
, ih.po_no
, ih.invoice_date
, (ih.total_amount - ih.freight) as amt
, count (il.line_no) as lines
, pt.location_id
, s.preferred_location_id
, ih.customer_id
, ih.bill2_name
, ih.ship_to_id
, ih.ship2_name
FROM p21_view_invoice_hdr ih
JOIN dbo.p21_view_invoice_line il ON il.invoice_no = ih.invoice_no
JOIN p21_view_ship_to s ON s.ship_to_id = ih.ship_to_id
JOIN p21_view_oe_pick_ticket pt ON pt.invoice_no = ih.invoice_no
JOIN p21_view_customer c ON c.customer_id = ih.customer_id
WHERE invoice_date BETWEEN {dateRange}
AND pt.location_id != s.preferred_location_id
AND ih.rma_flag = 'N'
AND ( c.class_1id != 'ADS' OR c.class_1id IS NULL )
GROUP BY ih.invoice_no
, ih.order_no
, ih.po_no
, ih.invoice_date
, ih.total_amount - ih.freight
, pt.location_id
, s.preferred_location_id
, ih.customer_id
, ih.bill2_name
, ih.ship_to_id
, ih.ship2_name
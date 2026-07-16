SELECT l.location_name
, ih.customer_id
, ih.bill2_name
, ih.ship_to_id
, ih.invoice_no
, ih.invoice_date
, pt.order_no
, rep.rep
, ih.freight
, ih.carrier_name
, ih.terms_desc
, invoice_batch_desc as invoice_desc
FROM p21_view_invoice_hdr ih
JOIN dbo.p21_view_oe_pick_ticket pt ON pt.invoice_no = ih.invoice_no
JOIN DA_Rep rep ON rep.customer_id = ih.customer_id
JOIN p21_view_location l ON l.location_id = pt.location_id
JOIN p21_view_invoice_batch ON p21_view_invoice_batch.invoice_batch_uid = pt.invoice_batch_uid
WHERE invoice_date BETWEEN {dateRange}
AND ih.bill2_name NOT LIKE 'ADS -%'
AND ih.total_amount > 0
AND pt.location_id = @locationId 
ORDER BY ih.invoice_no DESC
SELECT invoice_hdr.customer_id
, customer.customer_name
, invoice_hdr.ship_to_id
, address.name
, invoice_hdr.invoice_date
, invoice_hdr.invoice_no
, invoice_hdr.order_no
, invoice_hdr.po_no 
, oe_pick_ticket.pick_ticket_no as ticket_no
, ISNULL(carrier.name, '') as carrier
, ISNULL(tracking_no, '') as tracking_no
, total_amount
, rep
, DA_Rep.salesrep_id as rep_id
FROM customer WITH (NOLOCK)
JOIN invoice_hdr WITH (NOLOCK) ON invoice_hdr.customer_id = customer.customer_id
JOIN address WITH (NOLOCK) ON invoice_hdr.ship_to_id = address.id
JOIN DA_Rep ON DA_Rep.customer_id = customer.customer_id
JOIN dbo.oe_pick_ticket (NOLOCK) ON oe_pick_ticket.invoice_no = invoice_hdr.invoice_no
LEFT OUTER JOIN address carrier (NOLOCK) ON carrier.id = oe_pick_ticket.carrier_id
WHERE invoice_hdr.date_created BETWEEN {dateRange}
AND DA_Rep.salesrep_id = @repId
ORDER BY invoice_hdr.invoice_date DESC
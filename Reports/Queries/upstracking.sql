SELECT invoice_hdr.customer_id
,invoice_hdr.ship2_name
,invoice_hdr.invoice_no
,oe_pick_ticket.tracking_no
FROM invoice_hdr
JOIN oe_pick_ticket on oe_pick_ticket.invoice_no = invoice_hdr.invoice_no
JOIN address on oe_pick_ticket.carrier_id = address.id
WHERE invoice_hdr.invoice_no =@invoicenum
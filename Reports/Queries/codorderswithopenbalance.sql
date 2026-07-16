SELECT invoice_hdr.customer_id
, invoice_hdr.bill2_name
, invoice_hdr.terms_desc
, invoice_hdr.invoice_date
, invoice_hdr.invoice_no
, address.name as [service]
, oe_pick_ticket.tracking_no
, oe_pick_ticket.freight_out
, p21_invoice_amt_remaining_view.amt_remaining_frominv as [amount]
FROM invoice_hdr (NOLOCK)
JOIN p21_invoice_amt_remaining_view (NOLOCK) on p21_invoice_amt_remaining_view.invoice_no = invoice_hdr.invoice_no
JOIN oe_pick_ticket (NOLOCK) on oe_pick_ticket.invoice_no = invoice_hdr.invoice_no
JOIN address (NOLOCK) on oe_pick_ticket.carrier_id = address.id
WHERE invoice_hdr.paid_in_full_flag = 'N'
AND invoice_hdr.terms_desc like '%COD%'
AND address.name like '%UPS%'
AND datediff(dd,invoice_hdr.invoice_date, getdate()) > 14
AND oe_pick_ticket.tracking_no like '1Z%'
AND p21_invoice_amt_remaining_view.amt_remaining_frominv = invoice_hdr.total_amount
ORDER BY bill2_name,
invoice_hdr.invoice_date
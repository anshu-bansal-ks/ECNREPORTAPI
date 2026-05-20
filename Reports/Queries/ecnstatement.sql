SELECT invoice_hdr.invoice_date
, invoice_hdr.invoice_no
, ISNULL(oe_pick_ticket.tracking_no, '') as tracking_no
, ISNULL(invoice_hdr.po_no, '') as po_no
, CASE WHEN DATEDIFF(dd, invoice_hdr.invoice_date, GETDATE()) < 31 THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END AS [current]
, CASE WHEN DATEDIFF(dd, invoice_hdr.invoice_date, GETDATE()) BETWEEN 31 AND 60 THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END AS [31_to_60]
, CASE WHEN DATEDIFF(dd, invoice_hdr.invoice_date, GETDATE()) BETWEEN 61 AND 90 THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE  0 END AS [61_to_90]
, CASE WHEN DATEDIFF(dd, invoice_hdr.invoice_date, GETDATE()) > 90 THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END AS 'over_90'
, p21_invoice_amt_remaining_view.amt_remaining_frominv AS total
FROM customer WITH (NOLOCK)
JOIN p21_invoice_amt_remaining_view ON customer.customer_id = p21_invoice_amt_remaining_view.customer_id
JOIN invoice_hdr WITH (NOLOCK) ON p21_invoice_amt_remaining_view.invoice_no = invoice_hdr.invoice_no
LEFT OUTER JOIN oe_pick_ticket ON invoice_hdr.invoice_no = oe_pick_ticket.invoice_no
LEFT OUTER JOIN address AS add2 ON oe_pick_ticket.carrier_id = add2.id
WHERE invoice_hdr.customer_id = @custId
AND p21_invoice_amt_remaining_view.paid_in_full_flag = 'N'
ORDER BY invoice_hdr.invoice_date DESC
, invoice_hdr.invoice_no DESC
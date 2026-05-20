SELECT invoice_hdr.invoice_date, invoice_hdr.invoice_no,
oe_pick_ticket.tracking_no,
invoice_hdr.ship_to_id, address.name AS ship_to_name,
CASE WHEN datediff(dd, invoice_hdr.invoice_date,getdate()) < 31
THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END AS [current],
CASE WHEN datediff(dd, invoice_hdr.invoice_date, getdate()) BETWEEN 31 AND 60
THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END AS [31_to_60],
CASE WHEN datediff(dd, invoice_hdr.invoice_date, getdate()) BETWEEN 61 AND 90
THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END AS [61_to_90],
CASE WHEN datediff(dd, invoice_hdr.invoice_date, getdate()) > 90
THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END AS 'over_90',
p21_invoice_amt_remaining_view.amt_remaining_frominv AS total
FROM address
INNER JOIN invoice_hdr ON address.id = invoice_hdr.ship_to_id
INNER JOIN customer
INNER JOIN p21_invoice_amt_remaining_view ON customer.customer_id = p21_invoice_amt_remaining_view.customer_id
ON invoice_hdr.invoice_no = p21_invoice_amt_remaining_view.invoice_no
AND invoice_hdr.customer_id = customer.customer_id
Left Outer JOIN oe_pick_ticket ON invoice_hdr.invoice_no = oe_pick_ticket.invoice_no
Left JOIN address AS add2 ON oe_pick_ticket.carrier_id = add2.id
WHERE (invoice_hdr.customer_id = @custId) 
AND (p21_invoice_amt_remaining_view.paid_in_full_flag = 'N')
AND (invoice_hdr.consolidated = 'N')
ORDER BY invoice_hdr.ship_to_id, invoice_hdr.invoice_date DESC, invoice_hdr.invoice_no DESC
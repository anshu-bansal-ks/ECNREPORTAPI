SELECT p21_invoice_amt_remaining_view.customer_id
,customer.customer_name
,invoice_hdr.invoice_no
,oe_pick_ticket.tracking_no
,invoice_hdr.po_no
,invoice_hdr.invoice_date
,p21_invoice_amt_remaining_view.total_amount
FROM customer with(nolock)
join invoice_hdr with(nolock) on customer.customer_id = invoice_hdr.customer_id
Join oe_pick_ticket on  oe_pick_ticket.invoice_no = invoice_hdr.invoice_no
join p21_invoice_amt_remaining_view with(nolock) on p21_invoice_amt_remaining_view.invoice_no = invoice_hdr.invoice_no
Left JOIN address on oe_pick_ticket.carrier_id = address.id
WHERE customer.customer_id = @custId
and datediff(yy, invoice_hdr.invoice_date, getdate()) = 0
ORDER BY invoice_hdr.invoice_date,
invoice_hdr.invoice_no
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
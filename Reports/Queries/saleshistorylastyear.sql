SELECT p21_invoice_amt_remaining_view.customer_id,
customer.customer_name,
invoice_hdr.invoice_date,
invoice_hdr.invoice_no,
oe_pick_ticket.tracking_no,
p21_invoice_amt_remaining_view.total_amount,
-p21_invoice_amt_remaining_view.terms_taken - p21_invoice_amt_remaining_view.amount_paid + p21_invoice_amt_remaining_view.memo_amount as 'Adjustments',
p21_invoice_amt_remaining_view.amount_paid as payments,
p21_invoice_amt_remaining_view.amt_remaining_frominv as open_amount
FROM customer 
join invoice_hdr on customer.customer_id = invoice_hdr.customer_id  
join p21_invoice_amt_remaining_view on p21_invoice_amt_remaining_view.invoice_no = invoice_hdr.invoice_no
Join oe_pick_ticket on oe_pick_ticket.invoice_no = invoice_hdr.invoice_no
Left JOIN address on oe_pick_ticket.carrier_id = address.id
WHERE customer.customer_id=@custId 
And datediff (yy,invoice_hdr.invoice_date,getdate()) = 1
ORDER BY invoice_hdr.invoice_date,
invoice_hdr.invoice_no
SELECT p21_invoice_amt_remaining_view.customer_id
,customer.customer_name   
,invoice_hdr.ship_to_id   
,invoice_hdr.ship2_name  
,invoice_hdr.invoice_date 
,invoice_hdr.invoice_no   
,p21_invoice_amt_remaining_view.total_amount as invoice_total
FROM customer (NOLOCK)
JOIN invoice_hdr (NOLOCK) ON customer.customer_id = invoice_hdr.customer_id
JOIN p21_invoice_amt_remaining_view (NOLOCK) ON dbo.invoice_hdr.invoice_no = dbo.p21_invoice_amt_remaining_view.invoice_no
WHERE invoice_hdr.customer_id = customer.customer_id
AND customer.customer_id = @custId 
AND invoice_hdr.invoice_date BETWEEN {dateRange}
ORDER BY invoice_hdr.invoice_date
,invoice_hdr.invoice_no
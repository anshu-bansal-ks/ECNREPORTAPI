SELECT  invoice_hdr.customer_id
,customer.customer_name 
,invoice_hdr.invoice_date
,invoice_hdr.invoice_no 
,p21_invoice_amt_remaining_view.total_amount
,p21_invoice_amt_remaining_view.amount_paid
,p21_invoice_amt_remaining_view.amt_remaining_frominv as balance_due
FROM customer customer ( NOLOCK )
JOIN invoice_hdr (NOLOCK) ON customer.customer_id = dbo.invoice_hdr.customer_id
JOIN p21_invoice_amt_remaining_view (NOLOCK) ON dbo.invoice_hdr.invoice_no = dbo.p21_invoice_amt_remaining_view.invoice_no
WHERE invoice_hdr.customer_id = @custId 
AND p21_invoice_amt_remaining_view.paid_in_full_flag = 'N'
AND invoice_hdr.invoice_date <= @cutoffDate 
ORDER BY invoice_hdr.invoice_date
,invoice_hdr.invoice_no
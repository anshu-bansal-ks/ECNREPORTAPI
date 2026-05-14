select ar_receipts.remitter_id,
ar_receipts.date_received,
ar_payment_details.check_number,
ar_receipts_detail.customer_id,
customer.customer_name,
ar_receipts_detail.invoice_no,
invoice_hdr.invoice_date,
datediff(dd,invoice_hdr.invoice_date,ar_receipts.date_received) as age ,
ar_receipts_detail.payment_amount 
FROM ar_receipts with (nolock) 
join ar_receipts_detail with (nolock) on ar_receipts.receipt_number = ar_receipts_detail.receipt_number 
join customer with (nolock) on ar_receipts_detail.customer_id = customer.customer_id 
join ar_payment_details with (nolock) on ar_payment_details.payment_number = ar_receipts.payment_number 
join invoice_hdr with (nolock) on invoice_hdr.invoice_no = ar_receipts_detail.invoice_no
WHERE ar_receipts.payment_number = @payment_no
GROUP BY ar_receipts.remitter_id,
ar_receipts.payment_number,
ar_receipts.date_received,
ar_receipts_detail.customer_id,
customer.customer_name,
ar_receipts_detail.invoice_no,
ar_receipts_detail.payment_amount,
ar_payment_details.check_number,
invoice_hdr.invoice_date 
order by ar_receipts_detail.customer_id
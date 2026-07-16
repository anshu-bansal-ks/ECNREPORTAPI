SELECT ar_receipts.date_received AS 'Date'
, ar_payment_details.check_number 
, invoice_hdr.invoice_date 
, ar_receipts_detail.invoice_no 
, ar_receipts_detail.payment_amount AS 'Amount'
FROM ar_payment_details ar_payment_details
, ar_receipts ar_receipts
, ar_receipts_detail ar_receipts_detail
, invoice_hdr invoice_hdr
WHERE ar_receipts_detail.receipt_number = ar_receipts.receipt_number
AND ar_payment_details.payment_number = ar_receipts.payment_number
AND invoice_hdr.invoice_no = ar_receipts_detail.invoice_no
AND ar_payment_details.check_number=@check_number
AND ar_receipts_detail.customer_id=@custId
ORDER BY ar_payment_details.check_number
, ar_receipts_detail.invoice_no
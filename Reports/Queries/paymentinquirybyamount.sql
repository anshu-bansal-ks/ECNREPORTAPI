select ar_receipts_detail.customer_id
, address.name
, ar_receipts.date_received as 'date'
, ar_payment_details.check_number as 'check'
, ar_payment_details.payment_amount as 'amount'
FROM ar_receipts
JOIN ar_receipts_detail on ar_receipts.receipt_number = ar_receipts_detail.receipt_number
JOIN ar_payment_details on ar_payment_details.payment_number = ar_receipts.payment_number
JOIN address on ar_receipts.remitter_id = address.id
WHERE ar_payment_details.payment_amount = @amount_num
AND ar_receipts.date_received BETWEEN {dateRange}
GROUP BY ar_receipts.remitter_id
, ar_receipts.payment_number
, ar_receipts.date_received
, ar_receipts_detail.customer_id
, address.name
, ar_payment_details.check_number
, ar_payment_details.payment_amount
ORDER BY ar_receipts.date_received
, address.name
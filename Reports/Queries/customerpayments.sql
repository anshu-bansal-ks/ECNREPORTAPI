select ar_receipts.date_received
, ar_receipts.payment_number
,(sum(ar_receipts_detail.payment_amount)) as amount 
,ar_payment_details.check_number
,cast(round(ar_payment_details.payment_amount,2) as numeric(36,2)) as check_amount
FROM ar_receipts with (nolock)
join ar_receipts_detail with (nolock) on ar_receipts.receipt_number = ar_receipts_detail.receipt_number
join ar_payment_details with (nolock) on ar_payment_details.payment_number =  ar_receipts.payment_number
WHERE customer_id = @custId
GROUP BY ar_receipts.date_received
,ar_payment_details.check_number
, ar_receipts.payment_number
,ar_payment_details.payment_amount
ORDER BY ar_receipts.date_received desc
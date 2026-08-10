 SELECT ih.salesrep_id
, (c.first_name + ' ' + c.last_name) as Salesrep
, ih.invoice_no
, ISNULL(ih.po_no, '') as po_no
, ih.customer_id
, cust.customer_name
, ih.invoice_date
, rd.payment_date
, ih.paid_in_full_flag as paid_flag
, rd.payment_amount
, ih.freight
, ROUND( (CASE WHEN (ih.total_amount - ih.freight) = 0 THEN 100
ELSE (ih.total_amount - ih.shipping_cost) / (ih.total_amount - ih.freight) END ) , 2 ) as margin
, DATEDIFF(dd, invoice_date, rd.payment_date) as dayspaid
, ROUND( CASE WHEN ih.paid_in_full_flag = 'Y' 
THEN rd.payment_amount - freight ELSE rd.payment_amount END , 2 ) adjusted_amt
, CASE WHEN ( DATEDIFF(dd, invoice_date, rd.payment_date) <= @cutoff
OR rd.payment_amount < 0 ) THEN (rd.payment_amount - freight) ELSE 0 END AgedIn
, CASE WHEN ( DATEDIFF(dd, invoice_date, rd.payment_date) > @cutoff
AND rd.payment_amount > 0 ) THEN (rd.payment_amount - freight ) ELSE 0 END AgedOut
, CASE WHEN rd.payment_amount < 0 THEN 0 WHEN CASE WHEN (ih.total_amount - ih.freight) = 0 THEN 100
ELSE (ih.total_amount - ih.shipping_cost) / (ih.total_amount - ih.freight) END >= @margin THEN 0 ELSE
-ROUND( CASE WHEN ih.paid_in_full_flag = 'Y' THEN rd.payment_amount - ih.freight 
ELSE rd.payment_amount END, 2) END margin_adjust
, CASE WHEN rd.payment_amount - ih.freight < 0 THEN rd.payment_amount - ih.freight
WHEN ( DATEDIFF(dd, invoice_date, rd.payment_date) <= @cutoff OR rd.payment_amount < 0 ) THEN
(rd.payment_amount - freight) ELSE 0 END + CASE
WHEN rd.payment_amount - ih.freight < 0 THEN 0
WHEN CASE WHEN (ih.total_amount - ih.freight) = 0 THEN 100
ELSE (ih.total_amount - ih.shipping_cost) / (ih.total_amount - ih.freight) END >= @margin THEN 0
ELSE -ROUND( CASE WHEN ih.paid_in_full_flag = 'Y' THEN rd.payment_amount - ih.freight 
ELSE rd.payment_amount END, 2 ) END Bonus_Amt
FROM p21_view_invoice_hdr ih
JOIN p21_view_ar_receipts_detail rd ON rd.invoice_no = ih.invoice_no
JOIN contacts c ON c.id = ih.salesrep_id
JOIN p21_view_customer cust ON ih.customer_id = cust.customer_id
WHERE ( rd.payment_date BETWEEN {dateRange}
AND( @repId = 'ALL' OR ih.salesrep_id = @repId ) 
AND (ih.invoice_class <> 'INSTALL') )
AND (( po_no NOT LIKE '___-SH%'
AND po_no NOT LIKE '%MonthlyFeed%') OR ih.po_no IS NULL)
ORDER BY cust.customer_name
, ih.invoice_no
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
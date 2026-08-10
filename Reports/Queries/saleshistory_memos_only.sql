SELECT (c.first_name + ' ' + c.last_name) as Rep
,invoice_no
,ih.invoice_date 
,ih.ship2_name
,(ih.total_amount - freight) as amount
FROM invoice_hdr ih(NOLOCK )
JOIN dbo.invoice_hdr_salesrep ihs(NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c(NOLOCK) ON c.id = ihs.salesrep_id
WHERE ih.invoice_date BETWEEN {dateRange}                   
AND ihs.primary_salesrep = 'Y' 
AND ih.total_amount <> 0 
AND ih.order_no IS NULL
ORDER BY rep
,ih.ship2_name
,ih.invoice_date
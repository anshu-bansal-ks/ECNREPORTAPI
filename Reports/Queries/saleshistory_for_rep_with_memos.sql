SELECT ihs.salesrep_id
,(c.first_name + ' ' + c.last_name) as Rep
,SUM(ih.total_amount - freight) as sales
FROM invoice_hdr ih ( NOLOCK )
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep  = 'Y'
GROUP BY ihs.salesrep_id
,c.first_name + ' ' + c.last_name
SELECT ihs.salesrep_id as rep_id
, (c.first_name + ' ' + c.last_name) as Rep
, ih.ship2_name
, ih.po_no
, ih.invoice_no
, ih.invoice_date
, SUM(il.extended_price) as SALES
, FREIGHT.freight
, SUM(il.cogs_amount) as COST
, SUM(il.extended_price) - SUM(il.cogs_amount) as gross_profit
, CASE WHEN SUM(il.extended_price) = 0 THEN 0 ELSE
ROUND((SUM(il.extended_price) - SUM(il.cogs_amount)) / SUM(il.extended_price) * 100, 2)
END profit_percent
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN ( SELECT invoice_no
, freight
FROM invoice_hdr (NOLOCK) 
WHERE invoice_date BETWEEN {dateRange}
AND (po_no LIKE '%' +@po_no+ '%' or po_no LIKE  +@po_no+ '%')
) AS FREIGHT ON FREIGHT.invoice_no = ih.invoice_reference_no
WHERE ihs.primary_salesrep = 'Y'
AND (ih.po_no LIKE '%' +@po_no+ '%' or ih.po_no LIKE  +@po_no+ '%')    
And ih.invoice_date BETWEEN {dateRange}
GROUP BY ihs.salesrep_id
, ih.ship2_name
, ih.po_no
, ih.invoice_no
,ih.invoice_date
, c.first_name + ' ' + c.last_name
, c.last_name
, c.first_name
, FREIGHT.freight
ORDER BY ih.invoice_no
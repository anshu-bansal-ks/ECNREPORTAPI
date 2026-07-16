SELECT  ihs.salesrep_id
,(c.first_name + ' ' + c.last_name)  as rep
,SUM(il.extended_price) as sales
FROM invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
WHERE ih.invoice_date BETWEEN {dateRange} 
AND ihs.primary_salesrep = 'Y'
AND (ih.po_no like '%' +@po_no+ '%')            
GROUP BY ihs.salesrep_id
,c.first_name + ' ' + c.last_name
,c.last_name
,c.first_name           
ORDER BY c.last_name
,c.first_name
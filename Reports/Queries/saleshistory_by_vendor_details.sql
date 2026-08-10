SELECT s.supplier_id
,s.supplier_name
,CAST(SUM(qty_shipped) AS INT) as UNITS
,SUM(il.extended_price) as SALES   
,SUM(il.extended_price) - SUM(il.cogs_amount) as gross_profit
,CASE WHEN SUM(il.extended_price) = 0 THEN 0    
ELSE ROUND(( SUM(il.extended_price) - SUM(il.cogs_amount) ) / SUM(il.extended_price) * 100, 2)               
END profit_percent               
FROM invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no   
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id          
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id      
WHERE ih.invoice_date BETWEEN {dateRange}                       
AND ihs.primary_salesrep = 'Y'
GROUP BY s.supplier_id                
,s.supplier_name               
ORDER BY s.supplier_name
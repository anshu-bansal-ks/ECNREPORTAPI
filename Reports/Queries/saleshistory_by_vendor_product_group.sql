SELECT s.supplier_id
,s.supplier_name
,SUM(il.extended_price) as SALES
FROM invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
JOIN dbo.product_group AS pg (NOLOCK) ON pg.product_group_id = il.product_group_id
WHERE ih.invoice_date BETWEEN {dateRange} 	                                               
AND ihs.primary_salesrep = 'Y'
AND pg.product_group_desc = @product_group
GROUP BY s.supplier_id
,s.supplier_name
,il.product_group_id
,pg.product_group_desc
ORDER BY s.supplier_name
SELECT il.supplier_id
,s.supplier_name
,ih.customer_id
,c.customer_name
,ih.ship_to_id
,a.name ship_to_name
,DA_Rep.rep as Salesrep
,SUM(qty_shipped) as qty
,SUM(il.extended_price) as SALES
FROM invoice_hdr (NOLOCK) ih
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN customer (NOLOCK) c ON c.customer_id = ih.customer_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
JOIN address a (NOLOCK) ON a.id=ih.ship_to_id
LEFT OUTER JOIN da_rep ON DA_Rep.customer_id = c.customer_id
WHERE ih.invoice_date BETWEEN {dateRange}
And il.supplier_id = @supplierId
AND( @repId = 'ALL' OR DA_Rep.salesrep_id = @repId ) 
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND c.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND c.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (c.class_1id = 'B2B' OR c.class_1id NOT IN ('ADS', 'KIOSK') 
OR c.class_1id IS NULL)))
AND ihs.primary_salesrep = 'Y'
GROUP BY il.supplier_id
,s.supplier_name
,ih.customer_id
,c.customer_name
,ih.ship_to_id
,a.name
,DA_Rep.rep
,il.supplier_id
ORDER BY c.customer_name
SELECT ih.customer_id
,c.customer_name
,rep Salesrep  
,SUM(il.extended_price) as SALES
FROM invoice_hdr (NOLOCK) ih                                      
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN customer (NOLOCK) c ON c.customer_id = ih.customer_id   
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id   
LEFT OUTER JOIN da_rep ON DA_Rep.customer_id = c.customer_id 
Inner join {dashboard}.dbo.salsify_itemData slf on slf.item_id = il.item_id
WHERE ih.invoice_date BETWEEN {dateRange}
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND c.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND c.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (c.class_1id = 'B2B' OR c.class_1id NOT IN ('ADS', 'KIOSK') OR c.class_1id IS NULL))
)
AND ihs.primary_salesrep = 'Y'
AND (@mcat = '' OR slf.mcat = @mcat)
GROUP BY ih.customer_id
,c.customer_name
,rep           
ORDER BY c.customer_name

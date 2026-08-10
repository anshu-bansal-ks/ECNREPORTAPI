SELECT il.item_id              
,im.item_desc               
,CAST(SUM(qty_shipped) AS INT) as qty                                                  
FROM invoice_hdr ih ( NOLOCK )                                   
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id                          
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid                
JOIN customer (NOLOCK) cust ON cust.customer_id = ih.customer_id              
WHERE  ih.customer_id = @custId   
and ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
GROUP BY il.item_id                  
,im.item_desc                  
ORDER BY item_id
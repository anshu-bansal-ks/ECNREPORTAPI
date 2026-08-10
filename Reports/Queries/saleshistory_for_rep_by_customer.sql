SELECT  ihs.salesrep_id
,(c.first_name + ' ' + c.last_name) as Rep
,cus.customer_id
,cus.customer_name                   
,SUM(il.extended_price) SALES        
FROM invoice_hdr ih ( NOLOCK )      
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no 
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number 
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id          
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN customer (NOLOCK) cus ON cus.customer_id = ih.customer_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'  
AND ihs.salesrep_id=@repId
GROUP BY ihs.salesrep_id     
,c.first_name + ' ' + c.last_name
,cus.customer_id      
,cus.customer_name    
ORDER BY cus.customer_name
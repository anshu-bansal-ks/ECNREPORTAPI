SELECT ih.customer_id
,ih.bill2_name   
,SUM(il.extended_price) as SALES
FROM invoice_hdr ih ( NOLOCK )   
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
JOIN da_rep  (NOLOCK) ON DA_Rep.customer_id = ih.customer_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ih.customer_id in (select customer_id  
from {dashboard}.dbo.groupcodes
where groupcodes.groupcode = @groupcode 
and groupcodes.company = @compId  
AND (groupcodes.delete_flag = 0 or groupcodes.delete_flag is null) )
GROUP BY ih.customer_id,ih.bill2_name,rep 
ORDER BY bill2_name
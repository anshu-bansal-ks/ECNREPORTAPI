SELECT  ih.customer_id
,ih.bill2_name
,SUM(ih.total_amount - ih.freight) as PRODUCT
,SUM(ih.freight) as FREIGHT
,SUM(ih.total_amount) as TOTAL 
 FROM invoice_hdr ih(NOLOCK )
 JOIN da_rep(NOLOCK) ON DA_Rep.customer_id = ih.customer_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ih.customer_id in (select customer_id 
from {dashboard}.dbo.groupcodes
where groupcodes.groupcode =@grpCode 
and groupcodes.company = @CompId
AND (groupcodes.delete_flag = 0 or groupcodes.delete_flag is null) ) 
GROUP BY ih.customer_id 
,ih.bill2_name 
ORDER BY bill2_name
SELECT ih.ship_to_id
,ih.ship2_name
,(c.first_name + ' ' + c.last_name) as Rep
,CAST( ROUND( SUM ( CASE WHEN UPPER(@compId) IN ('PPM','IVD')
THEN il.qty_shipped / ISNULL(NULLIF(im.price10,0),1)ELSE il.qty_shipped END),0 ) AS INT) as qty
,SUM(il.extended_price) as SALES
FROM invoice_hdr ih(NOLOCK)
JOIN invoice_line(NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs(NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c(NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast(NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier(NOLOCK) s ON s.supplier_id = il.supplier_id
WHERE il.item_id = @itemId  
AND ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
GROUP BY ih.ship_to_id
,ih.ship2_name
,c.first_name + ' ' + c.last_name
ORDER BY ih.ship2_name


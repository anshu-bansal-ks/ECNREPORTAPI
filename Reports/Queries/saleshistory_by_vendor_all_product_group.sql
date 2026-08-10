SELECT s.supplier_id,
s.supplier_name,
Sum(il.extended_price) as SALES
FROM invoice_hdr ih ( nolock )
JOIN invoice_line (nolock) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( nolock ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( nolock ) ON c.id = ihs.salesrep_id
JOIN inv_mast (nolock) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (nolock) s ON s.supplier_id = il.supplier_id
JOIN dbo.product_group AS pg (nolock) ON pg.product_group_id = il.product_group_id
Inner join {dashboard}.dbo.salsify_itemData slf on slf.item_id = il.item_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
AND (@mcat = '' OR slf.mcat = @mcat)
AND (@scat = '' OR slf.scat = @scat) 
GROUP BY s.supplier_id,
s.supplier_name 
ORDER  BY s.supplier_name
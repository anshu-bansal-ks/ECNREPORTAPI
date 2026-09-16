SELECT im.item_id
, im.item_desc
, SUM(qty_shipped) as qty
, SUM(il.extended_price) as SALES
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
JOIN p21_view_oe_hdr oh ON oh.order_no = ih.order_no
JOIN {dashboard}.dbo.ShowsSupplier ss on ss.supplier_id = il.supplier_id
WHERE ih.invoice_date BETWEEN {dateRange} 
AND ihs.primary_salesrep = 'Y'
AND ShowId = @showId
AND ( @AllPO = 'true' OR @pono = '' OR ih.po_no LIKE '%' + @pono + '%')
AND ( @Alljobname = 'true' OR @job_name = '' OR job_name = @job_name )
GROUP BY im.item_id
, im.item_desc
ORDER BY im.item_id
, im.item_desc
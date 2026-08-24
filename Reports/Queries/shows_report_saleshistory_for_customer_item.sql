SELECT il.item_id,
im.item_desc,
Sum(qty_shipped) as qty,
Sum(il.extended_price) as SALES
FROM invoice_hdr ih ( nolock )
JOIN invoice_line (nolock) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( nolock ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( nolock ) ON c.id = ihs.salesrep_id
JOIN inv_mast (nolock) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN customer (nolock) cust ON cust.customer_id = ih.customer_id
JOIN p21_view_oe_hdr oh ON oh.order_no = ih.order_no
join {dashboard}.dbo.ShowsSupplier ss on ss.supplier_id = il.supplier_id 
WHERE ih.customer_id = @custId
AND ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
And ShowId = @showId 
AND ( @AllPO = 'true' OR @pono = '' OR ih.po_no LIKE '%' + @pono + '%')
AND ( @Alljobname = 'true' OR @job_name = '' OR job_name = @job_name )
GROUP BY ihs.salesrep_id,cust.customer_id, cust.customer_name,
c.first_name + ' ' + c.last_name,
c.last_name,c.first_name,il.item_id,im.item_desc
ORDER BY item_id
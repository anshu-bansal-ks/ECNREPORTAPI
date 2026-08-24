SELECT s.supplier_id
,s.supplier_name
,Sum(il.extended_price) as SALES
,SUM(IIF(ss.supplier_id IS NOT NULL, il.extended_price, 0)) as SHOW_SALES
,CASE WHEN SUM(IIF(ss.supplier_id IS NOT NULL,il.extended_price,0)) = 0 THEN 0 
ELSE ROUND(( SUM(IIF(ss.supplier_id IS NOT NULL,il.extended_price,0)) - SUM(IIF(ss.supplier_id IS NOT NULL,il.cogs_amount,0)) ) / SUM(IIF(ss.supplier_id IS NOT NULL,il.extended_price,0)) * 100, 2) 
END show_profit
,SUM(IIF(ss.supplier_id IS NULL, il.extended_price, 0)) as NON_SHOW_SALES 
,CASE WHEN SUM(IIF(ss.supplier_id IS NULL,il.extended_price,0)) = 0 THEN 0 
ELSE ROUND(( SUM(IIF(ss.supplier_id IS NULL,il.extended_price,0)) - SUM(IIF(ss.supplier_id IS NULL,il.cogs_amount,0)) ) / SUM(IIF(ss.supplier_id IS NULL,il.extended_price,0)) * 100, 2) 
END as non_show_profit
-- ,SUM(IIF(ss.supplier_id IS NOT NULL, (il.extended_price - il.cogs_amount), 0)) AS SHOW_GP
-- ,SUM(IIF(ss.supplier_id IS NULL, (il.extended_price - il.cogs_amount), 0)) AS NON_SHOW_GP
FROM invoice_hdr ih ( nolock )
JOIN invoice_line (nolock) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( nolock ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( nolock ) ON c.id = ihs.salesrep_id
JOIN inv_mast (nolock) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (nolock) s ON s.supplier_id = il.supplier_id
JOIN p21_view_oe_hdr oh ON oh.order_no = ih.order_no
Left join {dashboard}.dbo.ShowsSupplier ss on ss.supplier_id = il.supplier_id And ShowId = @showid 
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
AND ( @AllPO = 'true' OR @pono = '' OR ih.po_no LIKE '%' + @pono + '%')
AND ( @Alljobname = 'true' OR @job_name = '' OR job_name = @job_name )
GROUP BY s.supplier_id,s.supplier_name
ORDER BY s.supplier_name
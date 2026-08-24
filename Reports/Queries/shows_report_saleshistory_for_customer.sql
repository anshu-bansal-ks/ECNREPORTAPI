SELECT ih.customer_id,
ih.bill2_name,
rep,
Sum(il.extended_price) as SALES,
SUM(IIF(ss.supplier_id IS NOT NULL, il.extended_price, 0)) as SHOW_SALES,
SUM(IIF(ss.supplier_id IS NULL, il.extended_price, 0)) as NON_SHOW_SALES 
FROM invoice_hdr ih ( nolock )
JOIN customer (nolock) cust ON cust.customer_id = ih.customer_id
JOIN invoice_line (nolock) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( nolock ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( nolock ) ON c.id = ihs.salesrep_id
JOIN inv_mast (nolock) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (nolock) s ON s.supplier_id = il.supplier_id
JOIN da_rep (nolock)  ON da_rep.customer_id = ih.customer_id
Left join {dashboard}.dbo.ShowsSupplier ss on ss.supplier_id = il.supplier_id And ShowId = @showId 
JOIN p21_view_oe_hdr oh ON oh.order_no = ih.order_no  
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND cust.class_1id = 'ADS')
OR (@custclass = 'B2B' AND (cust.class_1id != 'ADS' OR cust.class_1id IS NULL))
)
AND ( @AllPO = 'true' OR @pono = '' OR ih.po_no LIKE '%' + @pono + '%')
AND ( @Alljobname = 'true' OR @job_name = '' OR job_name = @job_name )
GROUP BY ih.customer_id, ih.bill2_name,rep
ORDER  BY bill2_name
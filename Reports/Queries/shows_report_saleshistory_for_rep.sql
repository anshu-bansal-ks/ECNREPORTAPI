SELECT ihs.salesrep_id as rep_id
, (c.first_name + ' ' + c.last_name) as Rep 
, SUM(il.extended_price) as SALES
, SUM(IIF(ss.supplier_id IS NOT NULL, il.extended_price, 0)) as SHOW_SALES
, SUM(IIF(ss.supplier_id IS NULL, il.extended_price, 0)) as NON_SHOW_SALES
FROM invoice_hdr ih ( NOLOCK )
JOIN customer (NOLOCK) cust ON cust.customer_id = ih.customer_id
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no                    
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id            
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid 
Left join {dashboard}.dbo.ShowsSupplier ss on ss.supplier_id = il.supplier_id 
And ShowId = @showId
JOIN p21_view_oe_hdr oh ON oh.order_no = ih.order_no 
WHERE ih.invoice_date BETWEEN {dateRange} 
AND ihs.primary_salesrep = 'Y'
And ( @custclass = 'ALL' OR ( @custclass = 'ADS' AND cust.class_1id = 'ADS')
OR ( @custclass = 'KIOSK' AND cust.class_1id = 'KIOSK')
OR ( @custclass = 'B2B' AND (c.class_1id = 'B2B' OR c.class_1id NOT IN ('ADS', 'KIOSK') OR c.class_1id IS NULL))
)
AND ( @AllPO = 'true' OR @pono = '' OR ih.po_no LIKE '%' + @pono + '%')
AND ( @Alljobname = 'true' OR @job_name = '' OR job_name = @job_name )
GROUP BY ihs.salesrep_id
,c.first_name + ' ' + c.last_name 
,c.last_name
,c.first_name
ORDER BY c.last_name
,c.first_name
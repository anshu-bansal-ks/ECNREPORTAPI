SELECT il.item_id
,im.item_desc
,il.supplier_id
,SUM(il.extended_price) as SALES
,SUM(il.cogs_amount) as COST
,SUM(il.extended_price) - SUM(il.cogs_amount) as Gross_profit
,CASE WHEN SUM(il.extended_price) = 0 THEN 0
ELSE ROUND(( SUM(il.extended_price) - SUM(il.cogs_amount) )/ SUM(il.extended_price) * 100, 2) END profit_percent
FROM invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
WHERE ih.invoice_date BETWEEN {dateRange}
AND il.supplier_id = @supplierId
and ih.customer_id IN ( SELECT customer_id
FROM {dashboard}.dbo.groupcodes
WHERE groupcodes.groupcode = @grpCode
AND groupcodes.company = @CompId
AND ( groupcodes.delete_flag = 0
OR groupcodes.delete_flag IS NULL ))
GROUP BY il.item_id
,im.item_desc
,il.supplier_id
ORDER BY il.item_id
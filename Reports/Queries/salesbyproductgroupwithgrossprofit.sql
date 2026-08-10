SELECT pg.product_group_desc
, CAST(SUM (qty_shipped) AS INT) as UNITS
, SUM (il.extended_price) as SALES
, SUM (il.cogs_amount) as COST
, SUM (il.extended_price) - SUM (il.cogs_amount) as gross_profit
, CASE WHEN SUM (il.extended_price) = 0 THEN 0 ELSE
ROUND ((SUM (il.extended_price) - SUM (il.cogs_amount)) / SUM (il.extended_price) * 100, 2) END profit_percent
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
JOIN dbo.p21_view_product_group pg ON pg.product_group_id = il.product_group_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND pg.delete_flag = 'N'
GROUP BY pg.product_group_desc
ORDER BY pg.product_group_desc
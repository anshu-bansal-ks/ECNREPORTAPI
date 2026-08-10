SELECT s.supplier_id
, s.supplier_name
, SUM(il.extended_price) as SALES
, SUM(il.cogs_amount) as COST
, SUM(il.extended_price) - SUM(il.cogs_amount) as gross_profit
, CASE WHEN SUM(il.extended_price) = 0 THEN 0 ELSE
ROUND((SUM(il.extended_price) - SUM(il.cogs_amount)) / SUM(il.extended_price) * 100, 2)
END profit_percent
,ISNULL(featured.FEATURED_SALES,0) as FEATURED_SALES
,SUM(il.extended_price) - ISNULL(FEATURED.FEATURED_SALES, 0) as other_sales
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
LEFT OUTER JOIN (
SELECT il.supplier_id
, SUM(il.extended_price) FEATURED_SALES
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
LEFT OUTER JOIN inv_mast_ud (NOLOCK) ud ON ud.inv_mast_uid = il.inv_mast_uid
WHERE ih.invoice_date BETWEEN {dateRange}
AND ud.show = 'Y'
AND (ih.po_no LIKE '%' +@pono+ '%')
GROUP BY il.supplier_id) AS FEATURED ON FEATURED.supplier_id = s.supplier_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
AND (ih.po_no LIKE '%' +@pono+ '%')
GROUP BY s.supplier_id
, s.supplier_name
, FEATURED.FEATURED_SALES
ORDER BY s.supplier_name
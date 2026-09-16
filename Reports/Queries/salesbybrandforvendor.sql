SELECT s.supplier_id
, s.supplier_name
, im.parker_product_cd
, SUM(qty_shipped) AS UNITS
, SUM(il.extended_price) AS SALES
, SUM(il.cogs_amount) AS COST
, SUM(il.extended_price) - SUM(il.cogs_amount) AS gross_profit
, CASE WHEN SUM(il.extended_price) = 0 THEN 0 
ELSE ROUND((SUM(il.extended_price) - SUM(il.cogs_amount)) / SUM(il.extended_price) * 100, 2)
END AS profit_percent
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line il (NOLOCK) ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast im (NOLOCK) ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier s (NOLOCK) ON s.supplier_id = il.supplier_id
WHERE {subquery}
AND ihs.primary_salesrep = 'Y'
AND s.supplier_id = @supplierid
GROUP BY s.supplier_id,
s.supplier_name,
im.parker_product_cd
ORDER BY s.supplier_name
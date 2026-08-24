SELECT COALESCE(LY.supplier_id, CY.supplier_id) AS supplier_id,
COALESCE(SLY.supplier_name, SCY.supplier_name) AS supplier_name,
ISNULL(LY.SALES_LY, 0) AS SALES_LY,
ISNULL(CY.SALES_YTD, 0) AS SALES_YTD
FROM( SELECT il.supplier_id,
SUM(il.extended_price) AS SALES_LY
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line il (NOLOCK) ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast im (NOLOCK) ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier s (NOLOCK) ON s.supplier_id = il.supplier_id
WHERE ih.invoice_date  BETWEEN {lastYearDateRange}
AND ih.customer_id IN ( SELECT customer_id
FROM {dashboard}.dbo.groupcodes
WHERE groupcodes.groupcode = @group_code
AND groupcodes.company = @CompId
AND (groupcodes.delete_flag = 0 OR groupcodes.delete_flag IS NULL))
GROUP BY il.supplier_id) LY
FULL OUTER JOIN(
SELECT il.supplier_id,
SUM(il.extended_price) AS SALES_YTD
FROM p21_view_invoice_line il
JOIN p21_view_invoice_hdr ih ON ih.invoice_no = il.invoice_no
WHERE ih.customer_id IN ( SELECT customer_id
FROM {dashboard}.dbo.groupcodes
WHERE groupcodes.groupcode = @group_code
AND groupcodes.company = @CompId
AND (groupcodes.delete_flag = 0 OR groupcodes.delete_flag IS NULL))
AND ih.invoice_date BETWEEN {currentYearDateRange}
GROUP BY il.supplier_id) CY ON CY.supplier_id = LY.supplier_id
LEFT JOIN supplier SLY (NOLOCK) ON SLY.supplier_id = LY.supplier_id
LEFT JOIN supplier SCY (NOLOCK) ON SCY.supplier_id = CY.supplier_id
WHERE COALESCE(LY.supplier_id, CY.supplier_id) IS NOT NULL
ORDER BY COALESCE(SLY.supplier_name, SCY.supplier_name)
SELECT ih.customer_id
, ih.bill2_name
, ih.ship_to_id
, ih.ship2_name
, SUM(il.extended_price) as SALES
, rep
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
JOIN DA_Rep (NOLOCK) ON DA_Rep.customer_id = ih.customer_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ih.customer_id IN (
SELECT customer_id FROM dashboard.dbo.groupcodes
WHERE groupcodes.groupcode =@groupcode
AND groupcodes.company = @compId
AND
(
groupcodes.delete_flag = 0
OR groupcodes.delete_flag IS NULL
)
)
GROUP BY ih.customer_id
, ih.bill2_name
, ih.ship_to_id
, ih.ship2_name
, rep
ORDER BY bill2_name
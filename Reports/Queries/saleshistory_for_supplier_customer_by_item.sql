SELECT il.item_id
,il.item_desc
,ud.release_date
,CAST(SUM(qty_shipped) AS INT) as UNITS
,SUM(il.extended_price) as SALES
FROM invoice_hdr ih(NOLOCK)
JOIN invoice_line(NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs(NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c(NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast(NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier(NOLOCK) s ON s.supplier_id = il.supplier_id
LEFT OUTER JOIN inv_mast_ud ud ON ud.inv_mast_uid = im.inv_mast_uid
WHERE il.supplier_id = @supplierId
AND ih.customer_id = @custId
AND ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
GROUP BY il.item_id
,il.item_desc
,ud.release_date
ORDER BY il.item_id
,il.item_desc
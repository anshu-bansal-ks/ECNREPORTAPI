SELECT il.item_id
, im.item_desc
, im.price1
, ISNULL(ud.tester, 'N') as tester
, CAST(SUM (il.qty_shipped) AS INT) as qty
, SUM (il.extended_price) as sales
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no=il.invoice_no
JOIN dbo.invoice_hdr_salesrep (NOLOCK) ihsON ih.invoice_no=ihs.invoice_number
JOIN contacts (NOLOCK) c ON c.id=ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid=il.inv_mast_uid
LEFT OUTER JOIN inv_mast_ud (NOLOCK) ud ON ud.inv_mast_uid = im.inv_mast_uid
WHERE ih.invoice_date BETWEEN {dateRange}
AND il.supplier_id=@supplierId 
AND il.extended_price=0
AND (@stockable = 'false' OR (ud.tester = 'N' OR ud.tester IS NULL))
GROUP BY il.item_id
, im.item_desc
, im.price1
, ud.tester
ORDER BY il.item_id
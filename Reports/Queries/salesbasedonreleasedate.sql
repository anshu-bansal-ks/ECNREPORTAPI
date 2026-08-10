SELECT s.supplier_id
, s.supplier_name
, SUM(il.extended_price) as SALES
, ISNULL(b1.b1, 0) as [30_Days]
, ISNULL(b2.b2, 0) as [61_90]
, ISNULL(b3.b3, 0) as [91_356]
, SUM(il.extended_price) - ISNULL(b1.b1, 0) - ISNULL(b2.b2, 0) - ISNULL(b3.b3, 0) as [OVER1_YEAR]
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
LEFT OUTER JOIN ( SELECT il.supplier_id
, SUM(il.extended_price) b1
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
LEFT OUTER JOIN inv_mast_ud (NOLOCK) ud ON ud.inv_mast_uid = il.inv_mast_uid
WHERE ih.invoice_date BETWEEN {dateRange}
AND (ih.po_no LIKE '%' +@pono+ '%')
AND DATEDIFF(dd, ud.release_date, @releasedate) <= 30
GROUP BY il.supplier_id ) AS b1 ON b1.supplier_id = s.supplier_id
LEFT OUTER JOIN ( SELECT il.supplier_id
,SUM(il.extended_price) b2
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
LEFT OUTER JOIN inv_mast_ud (NOLOCK) ud ON ud.inv_mast_uid = il.inv_mast_uid
WHERE ih.invoice_date BETWEEN {dateRange}
AND (ih.po_no LIKE '%' +@pono+ '%')
AND DATEDIFF(dd, ud.release_date, @releasedate) > 30
AND DATEDIFF(dd, ud.release_date, @releasedate) <= 90
GROUP BY il.supplier_id ) AS b2 ON b2.supplier_id = s.supplier_id
LEFT OUTER JOIN ( SELECT il.supplier_id
, SUM(il.extended_price) b3
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
LEFT OUTER JOIN inv_mast_ud (NOLOCK) ud ON ud.inv_mast_uid = il.inv_mast_uid
WHERE ih.invoice_date BETWEEN {dateRange}
AND (ih.po_no LIKE '%' +@pono+ '%')
AND DATEDIFF(dd, ud.release_date, @releasedate) > 90
AND DATEDIFF(dd, ud.release_date, @releasedate) <= 365
GROUP BY il.supplier_id ) AS b3 ON b3.supplier_id = s.supplier_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
AND (ih.po_no LIKE '%' +@pono+ '%')
GROUP BY s.supplier_id
, s.supplier_name
, b1.b1
, b2.b2
, b3
, b3
ORDER BY s.supplier_name
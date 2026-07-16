SELECT cust.customer_id
, ih.bill2_name
, SUM(il.extended_price) as product_sales
, custtots.freight
, custtots.totamt as total
FROM invoice_hdr ih (NOLOCK)
JOIN dbo.customer (NOLOCK) cust ON cust.customer_id = ih.customer_id_number
AND cust.company_id = ih.company_no
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
JOIN DA_Rep (NOLOCK) ON DA_Rep.customer_id = ih.customer_id
JOIN
(
SELECT customer_id
, SUM(total_amount) totamt
, SUM(freight) freight
FROM invoice_hdr (NOLOCK)
WHERE invoice_date BETWEEN {dateRange} 
AND po_no LIKE '%' +@po_no+ '%'
GROUP BY customer_id
) AS custtots ON custtots.customer_id = cust.customer_id    
WHERE ih.invoice_date BETWEEN {dateRange} 
AND ihs.primary_salesrep = 'Y'
AND ih.po_no LIKE '%' +@po_no+ '%'
GROUP BY cust.customer_id
, ih.bill2_name
, custtots.totamt
, custtots.freight
, rep
ORDER BY bill2_name
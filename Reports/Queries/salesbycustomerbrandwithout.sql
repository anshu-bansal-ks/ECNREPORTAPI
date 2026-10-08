SELECT il.supplier_id,
s.supplier_name,
UPPER(im.parker_product_cd) AS parker_product_cd,
ih.customer_id,
c.customer_name,
rep AS Salesrep,
SUM(qty_shipped) AS UNITS,
SUM(il.extended_price) AS SALES
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line il (NOLOCK) ON ih.invoice_no = il.invoice_no
JOIN customer c (NOLOCK) ON c.customer_id = ih.customer_id
JOIN inv_mast im (NOLOCK) ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier s (NOLOCK) ON s.supplier_id = il.supplier_id
LEFT OUTER JOIN DA_Rep ON DA_Rep.customer_id = c.customer_id
WHERE {subquery}
AND il.supplier_id = @supplierid
AND im.parker_product_cd = @brand_name
GROUP BY il.supplier_id,
s.supplier_name,
im.parker_product_cd,
ih.customer_id,
c.customer_name,
rep
ORDER BY c.customer_name
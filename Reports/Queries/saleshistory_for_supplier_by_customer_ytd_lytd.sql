SELECT c.customer_id
,c.customer_name
,DA_Rep.rep  
,ISNULL(prodsales1.Sales1, 0) as lytd
,ISNULL(prodsales2.Sales2, 0) as ytd 
,ISNULL(prodsales2.Sales2, 0) - ISNULL(prodsales1.Sales1, 0) as Change 
,ISNULL(( ISNULL(prodsales2.Sales2, 0) - ISNULL(prodsales1.Sales1, 0) )/ NULLIF(prodsales1.Sales1, 0), 0) as 'percent'             
FROM customer (NOLOCK) c 
LEFT OUTER JOIN ( SELECT supplier_id 
,customer_id 
,SUM(p21_view_invoice_line.extended_price) AS 'Sales1'
FROM p21_view_invoice_hdr  (NOLOCK)
JOIN p21_view_invoice_line (NOLOCK) ON p21_view_invoice_line.invoice_no = p21_view_invoice_hdr.invoice_no
WHERE DATEDIFF(yy,p21_view_invoice_hdr.invoice_date,GETDATE()) = 1 
AND p21_view_invoice_hdr.invoice_date < DATEADD(yy,-1, GETDATE())
AND supplier_id = @supplierId
GROUP BY  supplier_id
,customer_id ) AS prodsales1 ON prodsales1.customer_id = c.customer_id
LEFT OUTER JOIN ( SELECT supplier_id
,customer_id
,SUM(p21_view_invoice_line.extended_price) AS 'Sales2'
FROM p21_view_invoice_hdr  (NOLOCK)
JOIN p21_view_invoice_line  (NOLOCK) ON p21_view_invoice_line.invoice_no = p21_view_invoice_hdr.invoice_no
WHERE DATEDIFF(yy,p21_view_invoice_hdr.invoice_date,GETDATE()) = 0
AND supplier_id = @supplierId
GROUP BY  supplier_id
,customer_id ) AS prodsales2 ON prodsales2.customer_id = c.customer_id
JOIN DA_Rep ON DA_Rep.customer_id = c.customer_id
WHERE Sales1 IS NOT NULL
OR Sales2 IS NOT NULL
ORDER BY  LTRIM(c.customer_name),
ISNULL(prodsales2.Sales2, 0) - ISNULL(prodsales1.Sales1, 0) ASC
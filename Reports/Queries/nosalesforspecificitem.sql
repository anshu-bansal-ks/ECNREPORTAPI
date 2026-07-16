SELECT c.customer_id
, c.customer_name
, r.rep
, s.LastSl as last_si
,ytd.ytd_sales
FROM p21_view_customer c
JOIN DA_Rep r ON r.customer_id = c.customer_id
JOIN dbo.da_ytd_static ytd ON ytd.customer_id = c.customer_id
JOIN dbo.DA_Cust_Stats_Static s ON s.customer_id = c.customer_id
WHERE s.LastSlDays <= 180
AND c.customer_id NOT IN (
SELECT DISTINCT
ih.customer_id
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
WHERE il.item_id LIKE @itemid 
AND ih.invoice_date BETWEEN {dateRange}
)
ORDER BY r.rep, c.customer_name
SELECT salesrep_id
, Rep
, customer_id
, customer_name
, ship_to_id
, ship2_name
, default_loc
, SUM(NJ) AS NJ
, SUM(FL) AS FL
, SUM(CA) AS CA 
, SUM(NJ)+Sum(FL)+SUM(CA) as Total 
FROM ( SELECT ihs.salesrep_id
, c.first_name + ' ' + c.last_name Rep
, cus.customer_id
, cus.customer_name
, ih.ship_to_id
, ih.ship2_name
, CASE
WHEN st.preferred_location_id = 100035 THEN 'NJ'
WHEN st.preferred_location_id = 100038 THEN 'FL'
WHEN st.preferred_location_id = 100105 THEN 'CA'
ELSE 'OTHER' END AS 'default_loc'
,SUM(IIF(b.branch_id= '01',il.extended_price,0)) AS [NJ]
,SUM(IIF(b.branch_id = '02',il.extended_price,0)) AS [FL]
,SUM(IIF(b.branch_id = '03',il.extended_price,0)) AS [CA]
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN customer (NOLOCK) cus ON cus.customer_id = ih.customer_id
JOIN dbo.p21_view_branch b ON (
b.branch_id = ih.branch_id
AND b.company_id = ih.company_no)
JOIN dbo.p21_view_ship_to st ON st.ship_to_id = ih.ship_to_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
AND ihs.salesrep_id = @repId
AND il.supplier_id= @vendorId 
GROUP BY ihs.salesrep_id
, c.first_name + ' ' + c.last_name
, cus.customer_id
, cus.customer_name
, ih.ship_to_id
, ih.ship2_name
, st.preferred_location_id
, b.branch_id
, b.branch_description
)BASE
GROUP BY salesrep_id, Rep, customer_id
, customer_name, ship_to_id, ship2_name, default_loc
ORDER BY customer_name, ship2_name
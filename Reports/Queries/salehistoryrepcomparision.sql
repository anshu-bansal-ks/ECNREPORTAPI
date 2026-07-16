WITH CurrentYear AS (
SELECT ihs.salesrep_id,
SUM(il.qty_shipped) AS CurrentYear_UNITS,
SUM(il.extended_price) AS CurrentYear_SALES
FROM invoice_hdr AS ih WITH (NOLOCK) 
JOIN customer AS cust WITH (NOLOCK) ON cust.customer_id = ih.customer_id 
JOIN invoice_line AS il WITH (NOLOCK) ON ih.invoice_no = il.invoice_no                     
JOIN invoice_hdr_salesrep AS ihs WITH (NOLOCK) ON ih.invoice_no = ihs.invoice_number            
JOIN inv_mast AS im WITH (NOLOCK) ON im.inv_mast_uid = il.inv_mast_uid   
WHERE ih.invoice_date BETWEEN {dateRange} 
AND ihs.primary_salesrep = 'Y' 
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND cust.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND cust.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (cust.class_1id = 'B2B' OR cust.class_1id NOT IN ('ADS', 'KIOSK') OR cust.class_1id IS NULL))
)
GROUP BY ihs.salesrep_id
),
PriorYear AS (
SELECT ihs.salesrep_id,
SUM(il.qty_shipped) AS PriorYear_UNITS,
SUM(il.extended_price) AS PriorYear_SALES
FROM invoice_hdr AS ih WITH (NOLOCK) 
JOIN customer AS cust WITH (NOLOCK) ON cust.customer_id = ih.customer_id 
JOIN invoice_line AS il WITH (NOLOCK) ON ih.invoice_no = il.invoice_no                     
JOIN invoice_hdr_salesrep AS ihs WITH (NOLOCK) ON ih.invoice_no = ihs.invoice_number           
JOIN inv_mast AS im WITH (NOLOCK) ON im.inv_mast_uid = il.inv_mast_uid   
WHERE ih.invoice_date BETWEEN {prevDateRange}
AND ihs.primary_salesrep = 'Y' 
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND cust.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND cust.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (cust.class_1id = 'B2B' OR cust.class_1id NOT IN ('ADS', 'KIOSK') OR cust.class_1id IS NULL))
)
GROUP BY ihs.salesrep_id)
SELECT cy.salesrep_id,
 c.first_name + ' ' + c.last_name AS Rep,
c.last_name,
c.first_name,
CAST(COALESCE(cy.CurrentYear_UNITS, 0) AS INT) AS qty_current,
COALESCE(cy.CurrentYear_SALES, 0) AS sales_current,
CAST(COALESCE(py.PriorYear_UNITS, 0) AS INT) AS qty_prior,
COALESCE(py.PriorYear_SALES, 0) AS sales_prior
FROM CurrentYear cy
LEFT JOIN PriorYear py ON cy.salesrep_id = py.salesrep_id
JOIN contacts AS c WITH (NOLOCK) ON c.id = cy.salesrep_id
ORDER BY c.last_name, c.first_name
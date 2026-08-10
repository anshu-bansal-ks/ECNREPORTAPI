SELECT  ihs.salesrep_id
,(c.first_name + ' ' + c.last_name) as Rep
,SUM(qty_shipped) as qty
,SUM(il.extended_price) as SALES
FROM invoice_hdr ih ( NOLOCK )
JOIN customer (NOLOCK) cust ON cust.customer_id = ih.customer_id
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no                    
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id            
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid  
WHERE ih.invoice_date BETWEEN {dateRange}
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND cust.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND cust.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (cust.class_1id = 'B2B' OR cust.class_1id NOT IN ('ADS', 'KIOSK') OR cust.class_1id IS NULL))
)
AND ihs.primary_salesrep = 'Y'
GROUP BY ihs.salesrep_id
,c.first_name + ' ' + c.last_name 
ORDER BY c.first_name + ' ' + c.last_name 
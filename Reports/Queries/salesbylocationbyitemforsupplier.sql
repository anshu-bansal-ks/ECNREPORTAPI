SELECT item_id
, item_desc
, Qty
, SUM(NJ) AS NJ
, SUM(FL) AS FL
, SUM(CA) AS CA 
, SUM(NJ)+Sum(FL)+SUM(CA) as Total 
FROM( SELECT il.item_id 
,il.item_desc
,SUM(IIF(l.location_id = '100035',il.extended_price,0))  AS [NJ]
,SUM(IIF(l.location_id = '100038',il.extended_price,0))  AS [FL]
,SUM(IIF(l.location_id = '100105',il.extended_price,0))  AS [CA]
,SUM(qty_shipped) Qty                                                    
FROM invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no   
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number 
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id                         
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid               
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
JOIN dbo.p21_view_oe_hdr oh (NOLOCK) ON oh.order_no = ih.order_no 
JOIN p21_view_inv_loc(NOLOCK) vi ON vi.inv_mast_uid = il.inv_mast_uid and vi.location_id = oh.source_location_id
JOIN dbo.p21_view_location l (NOLOCK) ON l.location_id = oh.source_location_id
JOIN customer (NOLOCK) cust ON cust.customer_id = ih.customer_id 
WHERE il.supplier_id = @supplierId 
AND ih.invoice_date BETWEEN {dateRange}                                        
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND cust.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND cust.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (cust.class_1id = 'B2B' OR cust.class_1id NOT IN ('ADS', 'KIOSK') OR cust.class_1id IS NULL))
)   
AND ihs.primary_salesrep = 'Y'  
GROUP BY il.item_id   
,il.item_desc )BASE
GROUP BY  item_id, item_desc,Qty
ORDER BY item_id ,item_desc
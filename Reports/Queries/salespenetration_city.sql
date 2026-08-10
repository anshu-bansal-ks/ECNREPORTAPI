SELECT dr.salesrep_id as rep_id
,dr.rep         
,customer.customer_id
,customer.customer_name
,a.phys_city as city       
,a.phys_state as 'state'  
,a.central_phone_number as phone
,a.email_address
,ISNULL(vs.Sales, 0) as Sales
,dcss.LastSl as last_sale      
FROM customer WITH ( NOLOCK ) 
JOIN dbo.DA_Rep AS dr ( NOLOCK ) ON dr.customer_id = customer.customer_id
JOIN dbo.DA_Cust_Stats_Static AS dcss ( NOLOCK ) ON dcss.customer_id = customer.customer_id
JOIN dbo.da_ytd_static AS dys ON dys.customer_id = customer.customer_id
JOIN dbo.address AS a  (NOLOCK) ON a.id = customer.customer_id
LEFT OUTER JOIN ( SELECT s.supplier_id
,s.supplier_name
,c.customer_id
,c.customer_name
,SUM(il.extended_price) Sales
FROM dbo.invoice_hdr AS ih ( NOLOCK )
JOIN dbo.invoice_line AS il ( NOLOCK ) ON il.invoice_no = ih.invoice_no
JOIN dbo.customer AS c ( NOLOCK ) ON c.customer_id = ih.customer_id
JOIN dbo.supplier AS s ( NOLOCK ) ON s.supplier_id = il.supplier_id
WHERE il.supplier_id = @supplierId
And ih.invoice_date BETWEEN {dateRange}								
GROUP BY s.supplier_id
,s.supplier_name
,c.customer_id
,c.customer_name
) AS vs ON vs.customer_id = customer.customer_id 
WHERE customer.delete_flag = 'N'
AND dr.salesrep_id NOT IN ( 5184, 1636, 10436, 10231, 1057, 13175, 1181, 7153, 5325, 5294 )
AND dcss.LastSlDays <= @salesDays 
ORDER BY dr.rep
,customer.customer_name
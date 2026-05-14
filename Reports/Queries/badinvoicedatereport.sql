SELECT invoice_no 
,order_no 
,bill2_name 
,invoice_date 
,order_date 
,date_created 
,created_by 
FROM invoice_hdr (nolock)
WHERE ( DATEDIFF(yy, date_created, invoice_date) > 1 
OR DATEDIFF(yy, date_created, invoice_date) < 0 ) 
AND DATEDIFF(mm, date_created, GETDATE()) < 3 
AND paid_in_full_flag = 'N'
AND order_no IS NOT null 
ORDER BY date_created
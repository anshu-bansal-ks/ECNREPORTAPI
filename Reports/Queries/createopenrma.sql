SELECT  hdr.source_location_id as location_id
,created_by             
,order_no            
,order_date          
,hdr.customer_id     
,hdr.ship2_name      
,hdr.po_no           
,rep                 
FROM oe_hdr (NOLOCK) hdr 
JOIN dbo.DA_Rep ON DA_Rep.customer_id = hdr.customer_id 
WHERE hdr.rma_flag = 'Y'        
AND hdr.approved = 'Y'    
AND hdr.completed = 'N'   
AND hdr.delete_flag = 'N' 
AND DATEDIFF(yyyy, order_date, GETDATE()) < 2
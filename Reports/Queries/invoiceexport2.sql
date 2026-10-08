SELECT  invoice_hdr.customer_id     
       ,customer.customer_name       
       ,invoice_hdr.ship_to_id       
       ,invoice_hdr.ship2_name       
       ,invoice_hdr.invoice_date     
       ,invoice_line.order_no        
       ,invoice_hdr.invoice_no       
       ,invoice_hdr.po_no            
       ,inv_mast.item_id             
       ,inv_mast.item_desc           
       ,inv_mast.price1              
       ,invoice_line.qty_requested   
       ,invoice_line.qty_shipped     
       ,invoice_line.unit_price      
       ,invoice_line.extended_price 
       ,ISNULL(v_upc.upc, 'NONEONFILE') AS upc 
       ,invoice_hdr.total_amount            
       ,invoice_hdr.freight                
       ,invoice_hdr.total_amount - invoice_hdr.freight AS productamt 
FROM    invoice_hdr (NOLOCK) 
        JOIN invoice_line (NOLOCK) ON invoice_hdr.invoice_no = invoice_line.invoice_no 
        JOIN inv_mast (NOLOCK) ON invoice_line.inv_mast_uid = inv_mast.inv_mast_uid    
        JOIN customer (NOLOCK) ON invoice_hdr.customer_id = customer.customer_id                
        LEFT OUTER JOIN v_upc (NOLOCK) ON inv_mast.inv_mast_uid = v_upc.inv_mast_uid   
WHERE   invoice_hdr.invoice_no = @invoicenum 
ORDER BY inv_mast.item_id
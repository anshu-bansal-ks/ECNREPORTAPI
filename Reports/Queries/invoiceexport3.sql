SELECT  invoice_hdr.customer_id
,customer.customer_name
,invoice_hdr.ship_to_id
,invoice_hdr.ship2_name
,invoice_hdr.invoice_date
,invoice_line.order_no 
,invoice_hdr.invoice_no
,invoice_hdr.po_no     
,invoice_line.item_id  
,invoice_line.item_desc
,ol.base_ut_price price1 
,invoice_line.qty_requested
,invoice_line.qty_shipped
,invoice_line.unit_price 
,(1 - ol.calc_value) * 100 as DiscPct
,invoice_line.extended_price   
,v_upc.upc             
,invoice_hdr.total_amount
,invoice_hdr.freight   
,invoice_hdr.total_amount - invoice_hdr.freight productamt 
FROM invoice_hdr (NOLOCK) 
JOIN invoice_line (NOLOCK) ON dbo.invoice_hdr.invoice_no = dbo.invoice_line.invoice_no
JOIN inv_mast (NOLOCK) ON dbo.invoice_line.inv_mast_uid = dbo.inv_mast.inv_mast_uid  
JOIN customer (NOLOCK) ON invoice_hdr.customer_id = customer.customer_id     
LEFT OUTER JOIN V_upc (NOLOCK) ON dbo.inv_mast.inv_mast_uid = dbo.v_upc.inv_mast_uid 
JOIN dbo.p21_view_oe_hdr (NOLOCK) oh ON oh.order_no = invoice_hdr.order_no      
JOIN dbo.p21_view_oe_line (NOLOCK) ol ON ( ol.order_no = invoice_hdr.order_no   
AND ol.line_no = invoice_line.oe_line_number) 
WHERE invoice_hdr.invoice_no = @invoicenum   
GROUP BY invoice_hdr.customer_id
,customer.customer_name  
,invoice_hdr.ship_to_id  
,invoice_hdr.ship2_name  
,invoice_hdr.invoice_date
,invoice_line.order_no   
,invoice_hdr.invoice_no  
,invoice_hdr.po_no 
,invoice_line.item_id
,invoice_line.item_desc  
,ol.base_ut_price
,invoice_line.qty_requested
,invoice_line.qty_shipped
,invoice_line.unit_price 
,(1 - ol.calc_value) * 100  
,invoice_line.extended_price 
,v_upc.upc       
,invoice_hdr.total_amount
,invoice_hdr.freight 
,invoice_hdr.total_amount - invoice_hdr.freight
 ORDER BY invoice_line.item_id
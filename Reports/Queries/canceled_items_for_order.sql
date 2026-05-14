SELECT  p21_view_oe_hdr.order_date
,p21_view_oe_hdr.order_no  
,p21_view_oe_hdr.customer_id
,p21_view_oe_hdr.ship2_name
,p21_view_oe_line.item_id  
,p21_view_inv_mast.item_desc
,CAST( p21_view_oe_line.qty_ordered AS INT) as qty_ordered
,CAST( p21_view_oe_line.qty_canceled AS INT) as qty_canceled
,upc                       
 FROM p21_view_oe_hdr (NOLOCK) 
 JOIN p21_view_oe_line (NOLOCK) ON p21_view_oe_line.order_no = p21_view_oe_hdr.order_no 
 JOIN p21_view_inv_mast (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_oe_line.inv_mast_uid 
 LEFT OUTER JOIN v_upc (NOLOCK) ON v_upc.inv_mast_uid = p21_view_inv_mast.inv_mast_uid 
 WHERE p21_view_oe_hdr.order_no = @ordernum 
 AND p21_view_oe_line.disposition = 'C'   
 AND p21_view_oe_hdr.delete_flag = 'N'
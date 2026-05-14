SELECT l.location_name
,p21_view_oe_line.order_no 
,p21_view_oe_hdr.customer_id
,p21_view_oe_hdr.ship2_name
,p21_view_inv_mast.item_id 
,p21_view_inv_mast.item_desc
,CAST(p21_view_oe_line.qty_ordered AS INT) AS qty_ordered
,CAST((p21_view_oe_line.qty_ordered - qty_invoiced - qty_on_pick_tickets - qty_canceled) AS INT) AS qty_backordered
,CAST((p21_view_inv_loc.qty_on_hand - p21_view_inv_loc.qty_allocated) AS INT) AS qty_available
,CASE WHEN p21_view_inv_loc.qty_on_hand- p21_view_inv_loc.qty_allocated = 0 THEN 'NOTHING TO ALLOCATE'
ELSE 'ALLOCATE' END AS 'Fillable'      
FROM p21_view_oe_hdr (NOLOCK)
JOIN p21_view_oe_line (NOLOCK) ON p21_view_oe_hdr.oe_hdr_uid = p21_view_oe_line.oe_hdr_uid  
JOIN p21_view_inv_mast (NOLOCK) ON p21_view_oe_line.inv_mast_uid = p21_view_inv_mast.inv_mast_uid      
JOIN p21_view_inv_loc (NOLOCK) ON ( p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid     
AND p21_view_inv_loc.location_id = p21_view_oe_hdr.source_location_id )
JOIN location (NOLOCK) l ON l.location_id = p21_view_oe_hdr.source_location_id
WHERE p21_view_oe_line.complete = 'N'
AND p21_view_oe_hdr.rma_flag = 'N'        
AND p21_view_oe_hdr.projected_order = 'N' 
AND p21_view_oe_line.disposition = 'b' 
AND p21_view_oe_hdr.ship2_name LIKE 'ads%'
AND DATEDIFF(mm, order_date, GETDATE()) < 5
ORDER BY Fillable                      
,location_name                  
,ship2_name                     
,p21_view_inv_mast.item_id
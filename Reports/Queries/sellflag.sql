SELECT l.location_name
,im.item_id   
,im.item_desc 
,il.stockable 
,il.sellable  
,il.buy       
FROM p21_view_inv_loc (NOLOCK) il 
JOIN p21_view_inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid 
JOIN p21_view_inventory_supplier (NOLOCK) supplier ON supplier.inv_mast_uid = im.inv_mast_uid 
JOIN dbo.location AS l  (NOLOCK)  ON l.location_id = il.location_id 
WHERE supplier.delete_flag = 'n' 
AND im.delete_flag = 'n' 
AND im.item_desc LIKE '%(disc)%' 
AND il.qty_on_hand + il.qty_in_transit + il.order_quantity = 0 
AND il.sellable = 'y' 
AND l.location_name NOT LIKE '%RETURNS%'  
ORDER BY il.item_id
, il.location_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
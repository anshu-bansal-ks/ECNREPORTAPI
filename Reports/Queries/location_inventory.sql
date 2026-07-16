SELECT p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, CAST((p21_view_inv_loc.qty_on_hand - qty_allocated) AS INT) as qty_available
, p21_view_inv_mast.price1 as price
, dbo.v_barcode.upc    
FROM p21_view_inv_loc (NOLOCK)
JOIN p21_view_inv_mast (NOLOCK) ON p21_view_inv_loc.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
LEFT OUTER JOIN dbo.v_barcode (NOLOCK) ON v_barcode.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
WHERE p21_view_inv_loc.location_id = @locationId                                                         
AND p21_view_inv_mast.delete_flag = 'n'                                                     
AND p21_view_inv_loc.qty_on_hand - qty_allocated >= @minqty                               
ORDER BY p21_view_inv_mast.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
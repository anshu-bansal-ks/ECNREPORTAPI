SELECT  p21_view_inv_mast.item_id
,p21_view_inv_mast.item_desc
,CAST(p21_view_inv_loc.qty_backordered AS INT) as qty_backordered
,p21_view_inv_loc.primary_bin
,p21_view_inv_loc.stockable
,p21_view_inv_loc.sellable
,p21_view_inv_loc.buy
,p21_view_inv_mast.delete_flag as suppress
FROM p21_view_inv_loc p21_view_inv_loc ( NOLOCK )
JOIN p21_view_inv_mast p21_view_inv_mast ( NOLOCK ) ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
WHERE (p21_view_inv_mast.item_desc LIKE '%(disc)%' OR p21_view_inv_mast.item_desc LIKE '%(spec)%')
AND p21_view_inv_loc.location_id = @locationId
AND p21_view_inv_loc.qty_on_hand = 0
AND p21_view_inv_mast.delete_flag ='N'
AND (@binzero = 'false' OR p21_view_inv_loc.primary_bin <> '0')
AND p21_view_inv_loc.primary_bin <> '00'
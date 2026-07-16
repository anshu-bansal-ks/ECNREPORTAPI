SELECT p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, p21_view_inv_loc.primary_bin
, CAST(p21_view_inv_loc.qty_on_hand as INT) as qty_on_hand
, CAST(p21_view_inv_loc.qty_backordered as INT) as qty_back_ordered
, CAST(p21_view_inv_loc.qty_allocated as INT) as qty_allocated
, p21_view_inv_loc.stockable 
, p21_view_inv_loc.sellable 
, CAST(p21_view_inv_loc.order_quantity as INT) as order_quantity
, p21_view_inv_loc.buy
, p21_view_inv_loc.discontinued
, inv_mast_ud.suppress_from_web as suppress_from
FROM inv_mast_ud inv_mast_ud
, p21_view_inv_loc p21_view_inv_loc
, p21_view_inv_mast p21_view_inv_mast
WHERE p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid 
AND p21_view_inv_mast.inv_mast_uid = inv_mast_ud.inv_mast_uid 
AND ((p21_view_inv_mast.item_desc Like '%disc%') 
AND (p21_view_inv_loc.location_id=@locationId) 
AND (p21_view_inv_loc.qty_on_hand=$.000000000) 
AND (p21_view_inv_loc.stockable='Y') 
AND (p21_view_inv_loc.order_quantity=$.000000000) 
OR (p21_view_inv_mast.item_desc Like '%disc%') 
AND (p21_view_inv_loc.location_id=@locationId) 
AND (p21_view_inv_loc.qty_on_hand=$0) 
AND (p21_view_inv_loc.order_quantity=$.000000000) 
AND (p21_view_inv_loc.sellable='Y') 
OR (p21_view_inv_mast.item_desc Like '%disc%') 
AND (p21_view_inv_loc.location_id=@locationId) 
AND (p21_view_inv_loc.qty_on_hand=$0) 
AND (p21_view_inv_loc.order_quantity=$.000000000) 
AND (p21_view_inv_loc.buy='Y'))
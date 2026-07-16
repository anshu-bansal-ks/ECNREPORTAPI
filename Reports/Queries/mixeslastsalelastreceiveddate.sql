SELECT p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, p21_view_inv_mast.price1
, CAST(p21_view_inv_loc.qty_on_hand AS INT) as qty
, p21_view_inv_loc.last_sale_date
, MAX(p21_view_item_lead_time.receipt_date) as last_received_date
FROM p21_view_item_lead_time (NOLOCK)
left outer JOIN p21_view_inv_mast (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_item_lead_time.inv_mast_uid
JOIN p21_view_inv_loc (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
WHERE p21_view_inv_mast.item_id LIKE 'dvd%'
AND p21_view_inv_loc.qty_on_hand > 0
AND p21_view_inv_loc.location_id = @locationId   
GROUP BY p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, p21_view_inv_mast.price1
, p21_view_inv_loc.qty_on_hand
, p21_view_inv_loc.last_sale_date
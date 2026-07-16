SELECT im.item_id
, im.item_desc
, im.price1
, CAST((il.qty_on_hand - il.qty_allocated)AS INT) as qty_on_hand
, CAST(il.last_rec_po AS INT ) as last_rec_po
FROM p21_view_inv_loc (NOLOCK) il
JOIN p21_view_inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
WHERE im.item_id LIKE '%MIX%'
AND il.qty_on_hand > $0 
AND il.location_id = @locationId
ORDER BY im.item_id
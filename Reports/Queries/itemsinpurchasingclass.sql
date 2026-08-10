SELECT im.item_id
, im.item_desc
, il.purchase_class
, ud.release_date
FROM p21_view_inv_mast im
JOIN p21_view_inv_loc il ON (
il.inv_mast_uid = im.inv_mast_uid
AND il.location_id = @locationId )
LEFT OUTER JOIN inv_mast_ud ud ON ud.inv_mast_uid = im.inv_mast_uid
WHERE purchase_class = @purchase_class
AND ud.release_date BETWEEN {dateRange} 
ORDER BY il.item_id
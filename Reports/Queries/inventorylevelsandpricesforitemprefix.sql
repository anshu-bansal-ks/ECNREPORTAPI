SELECT im.item_id
,im.item_desc
,vu.upc
,CAST((il.qty_on_hand - il.qty_allocated)As INT) as qty_available
,il.moving_average_cost as MAC_Cost
,il.standard_cost
,il.price1
,il.price8 as MSRP
,il.price7 as MAP
FROM inv_mast (NOLOCK) im
JOIN dbo.v_upc (NOLOCK) AS vu ON vu.inv_mast_uid = im.inv_mast_uid
JOIN inv_loc (NOLOCK) AS il ON il.inv_mast_uid = im.inv_mast_uid
WHERE il.location_id = @locationId
AND im.item_id LIKE @prefix + '%'
and im.delete_flag = 'N'
ORDER BY im.item_id
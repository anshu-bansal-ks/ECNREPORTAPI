SELECT im.item_id
, im.item_desc
, CAST(SUM(qty_on_hand) AS INT) as total_qty
, SUM(il.qty_on_hand * il.moving_average_cost) as inventory_value
FROM p21_view_inv_mast im
JOIN p21_view_inv_loc il ON il.inv_mast_uid = im.inv_mast_uid
WHERE im.delete_flag = 'N'
AND im.other_charge_item = 'N'
GROUP BY im.item_id
, im.item_desc
ORDER BY im.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
SELECT im.item_id
, im.item_desc
, im.price1
, costs.cost
FROM dbo.p21_view_inv_mast im
LEFT OUTER JOIN
(
SELECT inv_mast_uid
, MAX(standard_cost) cost
FROM p21_view_inv_loc
WHERE location_id IN (select value from {dashboard}.dbo.fn_CommaSeparatedStringToTable(@LocationList,',') )
GROUP BY inv_mast_uid
) AS costs ON im.inv_mast_uid = costs.inv_mast_uid
WHERE im.delete_flag = 'N'
AND im.other_charge_item = 'N'
ORDER BY im.item_id


-- WITH FilteredData AS (
--     SELECT 
--         im.item_id AS [item_id], 
--         im.item_desc AS [item_desc], 
--         im.price1 AS [price1], 
--         costs.cost AS [cost],
--         SUM(im.price1) OVER() AS [price1_total],
--         SUM(costs.cost) OVER() AS [cost_total],
--         COUNT(*) OVER() AS [total_records_count]
--     FROM dbo.p21_view_inv_mast im
--     LEFT OUTER JOIN (
--         SELECT 
--             inv_mast_uid, 
--             MAX(standard_cost) AS cost
--         FROM p21_view_inv_loc
--         WHERE location_id IN (
--             SELECT value 
--             FROM {dashboard}.dbo.fn_CommaSeparatedStringToTable(@LocationList, ',')
--         )
--         GROUP BY inv_mast_uid
--     ) AS costs ON im.inv_mast_uid = costs.inv_mast_uid
--     WHERE im.delete_flag = 'N'
--       AND im.other_charge_item = 'N'
-- )
-- SELECT 
--     item_id,
--     item_desc,
--     price1,
--     cost,
--     price1_total,
--     cost_total,
--     total_records_count
-- FROM FilteredData
-- ORDER BY item_id
-- OFFSET @Offset ROWS
-- FETCH NEXT @PageSize ROWS ONLY;
SELECT * FROM (
SELECT im.item_id
, im.item_desc
, s.supplier_name
, CAST(vp.supplier_id AS INT) as Primary_Supplier
, l.location_id
, l.location_name
, imunl.release_date
, ISNULL(invusage.usage, 0) as usage
, (il.qty_on_hand - il.qty_allocated) as qty_available
, CASE WHEN (il.qty_on_hand - il.qty_allocated) - usage > 0
THEN (il.qty_on_hand - il.qty_allocated) - usage ELSE 0 END AS overstock
-- , CAST(CASE WHEN (il.qty_on_hand - il.qty_allocated) - usage > 0
-- THEN (il.qty_on_hand - il.qty_allocated) - usage ELSE 0 END AS INT ) AS overstock
, il.standard_cost
FROM inv_mast(NOLOCK) im
JOIN inv_loc(NOLOCK) il ON (il.inv_mast_uid = im.inv_mast_uid
AND il.location_id = @locationId )
JOIN dbo.inv_mast_ud AS imunl ON imunl.inv_mast_uid = im.inv_mast_uid
JOIN dbo.v_supplier_x_loc AS vp(NOLOCK) ON ( vp.item_id = im.item_id
AND vp.location_id = @locationId )
JOIN supplier(NOLOCK) s ON s.supplier_id = vp.supplier_id
JOIN dbo.location AS l(NOLOCK) ON l.location_id = @locationId
LEFT OUTER JOIN ( SELECT usage.item_id
, usage.location_id
, SUM(inv_period_usage) usage
FROM p21_view_inv_period_usage(NOLOCK) usage
JOIN p21_view_demand_period ON usage.demand_period_uid = p21_view_demand_period.demand_period_uid
WHERE usage.item_id IN ( SELECT DISTINCT im.item_id
FROM inv_mast(NOLOCK) im
JOIN inv_loc(NOLOCK) il ON il.inv_mast_uid = im.inv_mast_uid
JOIN dbo.tbl_invty_stats_static AS vis(NOLOCK) ON vis.item_id = im.item_id 
AND vis.location_id = @locationId
JOIN dbo.inv_mast_ud AS imunl ON imunl.inv_mast_uid = im.inv_mast_uid
JOIN dbo.v_supplier_x_loc AS vp(NOLOCK) ON ( vp.item_id = im.item_id
AND vp.location_id = @locationId )
WHERE vp.location_id = @locationId
AND vp.supplier_id = @supplierId)
AND (( @begyr <> @endyr
AND (( p21_view_demand_period.period >= @begper
AND p21_view_demand_period.year_for_period = @begyr )
OR ( p21_view_demand_period.year_for_period > @begyr
AND p21_view_demand_period.year_for_period < @endyr )
OR ( p21_view_demand_period.period <= @endper
AND p21_view_demand_period.year_for_period = @endyr)))
OR ( @begyr = @endyr
AND @begyr = p21_view_demand_period.year_for_period
AND p21_view_demand_period.period BETWEEN @begper AND @endper))
AND usage.location_id = @locationId
GROUP BY usage.item_id
, usage.location_id ) AS invusage ON invusage.item_id = im.item_id
WHERE l.location_id = @locationId
AND il.qty_on_hand - il.qty_allocated > 0 ) AS DATA
WHERE overstock > 0
ORDER BY supplier_name
, location_id
, item_id

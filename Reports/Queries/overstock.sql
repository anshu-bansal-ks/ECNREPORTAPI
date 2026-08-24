SELECT im.item_id
, im.item_desc
, s.supplier_name
, CAST(vp.supplier_id AS INT) as Primary_Supplier
, l.location_id
, l.location_name
, imunl.release_date
, CAST(ISNULL(invusage.usage, 0) AS INT) as usage
, il.order_quantity as qty_on_po
, il.qty_in_transit as qty_on_transfer
, il.qty_on_hand - il.qty_allocated as qty_available
, (il.qty_on_hand - il.qty_allocated) * il.standard_cost as value
, il.standard_cost
, vis.first_received
, vis.last_received
, vis.times_received
FROM inv_mast(NOLOCK) im
JOIN inv_loc(NOLOCK) il ON ( il.inv_mast_uid = im.inv_mast_uid AND il.location_id = @locationId)
JOIN dbo.tbl_invty_stats_static AS vis(NOLOCK) ON vis.item_id = im.item_id AND vis.location_id = @locationId
JOIN dbo.inv_mast_ud AS imunl ON imunl.inv_mast_uid = im.inv_mast_uid
JOIN dbo.v_supplier_x_loc AS vp(NOLOCK) ON ( vp.item_id = im.item_id AND vp.location_id = @locationId)
JOIN supplier(NOLOCK) s ON s.supplier_id = vp.supplier_id
JOIN dbo.location AS l(NOLOCK) ON l.location_id = @locationId
LEFT OUTER JOIN ( SELECT usage.item_id
,usage.location_id
,SUM(inv_period_usage) usage
FROM p21_view_inv_period_usage(NOLOCK) usage
JOIN p21_view_demand_period ON usage.demand_period_uid = p21_view_demand_period.demand_period_uid
WHERE usage.item_id IN ( SELECT DISTINCT im.item_id
FROM inv_mast(NOLOCK) im
JOIN inv_loc(NOLOCK) il ON il.inv_mast_uid = im.inv_mast_uid
JOIN dbo.tbl_invty_stats_static AS vis(NOLOCK) ON vis.item_id = im.item_id AND vis.location_id = @locationId
JOIN dbo.inv_mast_ud AS imunl ON imunl.inv_mast_uid = im.inv_mast_uid
JOIN dbo.v_supplier_x_loc AS vp(NOLOCK) ON ( vp.item_id = im.item_id AND vp.location_id = @locationId)
WHERE imunl.release_date BETWEEN @startrelease AND @endrelease
AND vis.times_received <= @maxXrecd
AND vp.supplier_id = @supplierId
AND vp.location_id = @locationId)
AND (( @begyr <> @endyr
AND (( p21_view_demand_period.period >= @begper
AND p21_view_demand_period.year_for_period = @begyr)
OR ( p21_view_demand_period.year_for_period > @begyr
AND p21_view_demand_period.year_for_period < @endyr)
OR ( p21_view_demand_period.period <= @endper 
AND p21_view_demand_period.year_for_period = @endyr)))
OR ( @begyr = @endyr
AND @begyr = p21_view_demand_period.year_for_period
AND p21_view_demand_period.period BETWEEN @begper AND @endper))
AND usage.location_id = @locationId
GROUP BY usage.item_id
,usage.location_id
) AS invusage ON invusage.item_id = im.item_id
WHERE imunl.release_date BETWEEN @startrelease AND @endrelease
AND vis.times_received <= @maxXrecd
AND vp.supplier_id = @supplierId
AND vp.location_id = @locationId
AND l.location_id = @locationId
ORDER BY s.supplier_name
,l.location_id
,im.item_id
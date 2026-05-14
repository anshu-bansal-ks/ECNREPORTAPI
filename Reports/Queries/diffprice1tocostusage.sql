SELECT p21_view_inv_mast.item_id,
p21_view_inv_mast.item_desc,
cast(Sum(p21_view_inv_period_usage.inv_period_usage) AS int) AS usage,
p21_view_inv_mast.price1,
p21_view_inv_loc.standard_cost,
(p21_view_inv_mast.price1 - p21_view_inv_loc.standard_cost) as price_cost_diff
FROM p21_view_demand_period p21_view_demand_period,
p21_view_inv_loc p21_view_inv_loc,
p21_view_inv_mast p21_view_inv_mast,
p21_view_inv_period_usage p21_view_inv_period_usage
WHERE p21_view_inv_loc.inv_mast_uid = p21_view_inv_period_usage.inv_mast_uid 
AND p21_view_inv_period_usage.demand_period_uid = p21_view_demand_period.demand_period_uid
AND p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid 
AND ((p21_view_inv_loc.location_id=@locationId)
AND p21_view_demand_period.beginning_date>=@startperiod 
and p21_view_demand_period.ending_date<=@endperiod
and ((p21_view_inv_mast.price1 - p21_view_inv_loc.standard_cost) > '9.99')
AND (p21_view_inv_period_usage.location_id=@locationId))
GROUP BY p21_view_inv_mast.item_id,
p21_view_inv_mast.item_desc,
p21_view_inv_mast.price1,p21_view_inv_loc.standard_cost
order by p21_view_inv_mast.item_id,
p21_view_inv_mast.item_desc,
p21_view_inv_mast.price1,p21_view_inv_loc.standard_cost
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
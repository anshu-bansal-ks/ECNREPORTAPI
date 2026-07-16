SELECT p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, p21_view_inv_mast.price1
, p21_view_inventory_supplier.cost
, p21_view_inventory_supplier.supplier_id
, p21_view_supplier.supplier_name
, CAST(p21_view_inv_loc.qty_on_hand AS INT) AS qty_on_hand
, CAST(p21_view_inv_loc.qty_allocated AS INT) AS qty_allocated
, p21_view_inv_loc.last_sale_date
, p21_view_inv_loc.last_purchase_date
, inv_mast_ud.release_date
, CAST(SUM(p21_view_inv_period_usage.inv_period_usage) AS INT) AS 'SUM'
, CAST(MAX(p21_view_inv_period_usage.inv_period_usage) AS INT) AS 'MAX'
, CAST(AVG(p21_view_inv_period_usage.inv_period_usage) AS INT) AS 'AVG'
, CAST(((AVG(p21_view_inv_period_usage.inv_period_usage)) * 12) AS INT) AS 'years_usage'
, CAST((p21_view_inv_loc.qty_on_hand - (AVG(p21_view_inv_period_usage.inv_period_usage) * 12)) AS INT) AS 'overstock'
, p21_view_inv_loc.primary_bin
, p21_view_inv_mast.default_sales_discount_group
FROM p21_view_inv_mast (NOLOCK)
JOIN p21_view_inv_loc (NOLOCK) ON p21_view_inv_loc.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
JOIN p21_view_inv_period_usage WITH (NOLOCK) ON (
p21_view_inv_period_usage.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
AND p21_view_inv_period_usage.location_id = p21_view_inv_loc.location_id)
JOIN p21_view_demand_period (NOLOCK) ON p21_view_inv_period_usage.demand_period_uid = p21_view_demand_period.demand_period_uid
LEFT JOIN inv_mast_ud (NOLOCK) ON inv_mast_ud.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
JOIN p21_view_inventory_supplier (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inventory_supplier.inv_mast_uid
JOIN p21_view_supplier (NOLOCK) ON p21_view_supplier.supplier_id = p21_view_inventory_supplier.supplier_id
JOIN dbo.inventory_supplier_x_loc ON (inventory_supplier_x_loc.inventory_supplier_uid = p21_view_inventory_supplier.inventory_supplier_uid AND inventory_supplier_x_loc.location_id = p21_view_inv_loc.location_id)
WHERE p21_view_demand_period.beginning_date>=@startPeriod and ending_date<=@endPeriod
AND p21_view_inv_period_usage.location_id = @locationId
AND ( @supplierId = 'ALL' OR p21_view_inventory_supplier.supplier_id = TRY_CAST(@supplierId AS INT))
AND p21_view_inventory_supplier.delete_flag = 'n'
AND p21_view_inv_mast.delete_flag = 'N'
AND primary_supplier = 'Y'
GROUP BY p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, p21_view_inv_mast.price1
, p21_view_inventory_supplier.cost
, p21_view_inventory_supplier.supplier_id
, p21_view_supplier.supplier_name
, p21_view_inv_loc.qty_on_hand
, p21_view_inv_loc.qty_allocated
, p21_view_inv_loc.last_sale_date
, p21_view_inv_loc.last_purchase_date
, inv_mast_ud.release_date
, p21_view_inv_loc.primary_bin
, p21_view_inv_mast.default_sales_discount_group
ORDER BY p21_view_inv_mast.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
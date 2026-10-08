SELECT 
    inv_mast.item_id,
    inv_mast.item_desc,
    inv_mast.price1,
    p21_view_inventory_supplier.cost,
    p21_view_inv_loc.moving_average_cost AS average_cost,
    p21_view_inventory_supplier.supplier_id,
    p21_view_supplier.supplier_name,
    p21_view_inv_loc.qty_on_hand,
    p21_view_inv_loc.qty_allocated,
    CASE WHEN p21_view_inv_loc.last_sale_date = '1990/1/1' THEN NULL ELSE p21_view_inv_loc.last_sale_date END AS last_sale_date,
    p21_view_inv_loc.last_purchase_date,
    inv_mast_ud.release_date,
    SUM(ISNULL(p21_view_inv_period_usage.inv_period_usage, 0)) AS [sum],
    MAX(ISNULL(p21_view_inv_period_usage.inv_period_usage, 0)) AS [max],
    AVG(ISNULL(p21_view_inv_period_usage.inv_period_usage, 0)) AS [avg],
    (AVG(ISNULL(p21_view_inv_period_usage.inv_period_usage, 0)) * 12) AS years_usage,
    (p21_view_inv_loc.qty_on_hand - (AVG(ISNULL(p21_view_inv_period_usage.inv_period_usage, 0)) * 12)) AS overstock,
    p21_view_inv_loc.primary_bin,
    inv_mast.default_sales_discount_group AS default_sales
FROM inv_mast WITH (NOLOCK)
JOIN p21_view_inv_loc WITH (NOLOCK) ON p21_view_inv_loc.inv_mast_uid = inv_mast.inv_mast_uid AND p21_view_inv_loc.location_id = @locationid
LEFT JOIN inv_mast_ud WITH (NOLOCK) ON inv_mast_ud.inv_mast_uid = inv_mast.inv_mast_uid
JOIN p21_view_inventory_supplier WITH (NOLOCK) ON p21_view_inventory_supplier.inv_mast_uid = inv_mast.inv_mast_uid
JOIN p21_view_supplier WITH (NOLOCK) ON p21_view_supplier.supplier_id = p21_view_inventory_supplier.supplier_id
JOIN dbo.inventory_supplier_x_loc ON inventory_supplier_x_loc.inventory_supplier_uid = p21_view_inventory_supplier.inventory_supplier_uid AND inventory_supplier_x_loc.location_id = p21_view_inv_loc.location_id
LEFT JOIN p21_view_inv_period_usage WITH (NOLOCK) ON p21_view_inv_period_usage.inv_mast_uid = inv_mast.inv_mast_uid AND p21_view_inv_period_usage.location_id = @locationid
WHERE p21_view_inv_loc.location_id = @locationid
  AND p21_view_inventory_supplier.delete_flag = 'n'
  AND inv_mast.delete_flag = 'N'
  AND primary_supplier = 'Y'
  {supplierQuery}
  {binQuery}
  {discQuery}
GROUP BY 
    inv_mast.item_id,
    inv_mast.item_desc,
    inv_mast.price1,
    p21_view_inventory_supplier.cost,
    p21_view_inv_loc.moving_average_cost,
    p21_view_inventory_supplier.supplier_id,
    p21_view_supplier.supplier_name,
    p21_view_inv_loc.qty_on_hand,
    p21_view_inv_loc.qty_allocated,
    p21_view_inv_loc.last_sale_date,
    p21_view_inv_loc.last_purchase_date,
    inv_mast_ud.release_date,
    p21_view_inv_loc.primary_bin,
    inv_mast.default_sales_discount_group
ORDER BY inv_mast.item_id
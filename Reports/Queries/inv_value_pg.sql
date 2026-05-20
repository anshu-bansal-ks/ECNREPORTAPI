SELECT  p21_view_inventory_value_report.product_group_desc AS product_group
,cast(CASE WHEN cost_basis = 'FIFO' THEN ROUND(SUM(fifo_layer_qty), 2)
ELSE ROUND(SUM(qty_on_hand), 2) END AS INT) AS qty
,CASE WHEN cost_basis = 'FIFO' THEN ROUND(SUM(fifo_layer_value), 2)
ELSE ROUND(SUM(qty_on_hand * cost), 2) END AS [value]
FROM p21_view_inventory_value_report  (NOLOCK)
WHERE ( p21_view_inventory_value_report.qty_on_hand <> $0
OR fifo_layer_qty <> 0 )
GROUP BY p21_view_inventory_value_report.product_group_desc, cost_basis
ORDER BY p21_view_inventory_value_report.product_group_desc
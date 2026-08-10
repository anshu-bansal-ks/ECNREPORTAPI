SELECT primary_supplier_id supplier_id
, p21_view_inventory_value_report.supplier_name 
, CASE WHEN cost_basis = 'FIFO' THEN ROUND(SUM(fifo_layer_qty), 2) ELSE ROUND(SUM(qty_on_hand), 2)
END AS qty
, CASE WHEN cost_basis = 'FIFO' THEN ROUND(SUM(fifo_layer_value), 2) ELSE ROUND(SUM(qty_on_hand * cost), 2)
END AS mac_value
FROM p21_view_inventory_value_report (NOLOCK)
WHERE ( p21_view_inventory_value_report.qty_on_hand <> 0
OR fifo_layer_qty <> 0 )
AND ( (@locationId = 'ALL' AND p21_view_inventory_value_report.location_id IN
( SELECT location_id FROM dbo.p21_view_location WHERE Company_id = 'ECN' AND delete_flag = 'N' ))
OR (@locationId <> 'ALL'  AND p21_view_inventory_value_report.location_id = @locationId ))
GROUP BY primary_supplier_id
, p21_view_inventory_value_report.supplier_name
, cost_basis
ORDER BY p21_view_inventory_value_report.supplier_name


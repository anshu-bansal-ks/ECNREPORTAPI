SELECT p21_item_location_view.supplier_name,
p21_item_location_view.item_id,
p21_item_location_view.item_desc,
p21_item_location_view.qty_on_hand,
p21_item_location_view.qty_allocated,
ROUND(p21_item_location_view.moving_average_cost,2) as average_cost,
ROUND(qty_on_hand * moving_average_cost, 2) as inventory_value
FROM p21_item_location_view 
JOIN location (NOLOCK) ON location.location_id = p21_item_location_view.location_id 
WHERE p21_item_location_view.delete_flag = 'N'
AND p21_item_location_view.qty_on_hand <> $0 
AND location.delete_flag = 'N' 
AND location.location_id = @locationId  
ORDER BY supplier_name, 
item_id
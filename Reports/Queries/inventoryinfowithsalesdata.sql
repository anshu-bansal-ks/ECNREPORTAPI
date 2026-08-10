SELECT il.location_id
, l.location_name
, im.item_id
, im.item_desc
, pg.product_group_desc
, il.stockable
, il.sellable
, il.buy
, il.discontinued
, il.qty_on_hand
, il.qty_allocated
, il.price1
, CONCAT(CAST(CAST(il.moving_average_cost AS DECIMAL(18,2)) AS VARCHAR(20)), '%') AS average_cost
, CAST(il.period_first_stocked AS INT ) as period_first_stocked
, CAST(il.year_first_stocked AS INT) as year_first_stocked
, il.last_sale_date
, il.last_purchase_date
, s.supplier_id
, supplier_name
, CAST(il.order_quantity AS INT) as order_quantity
, ISNULL([6_month].qty_sold, 0) as qty_sold
, ISNULL([6_month].sales, 0) as sales_dollars
FROM dbo.p21_view_inv_mast im
JOIN dbo.p21_view_inv_loc il ON il.inv_mast_uid = im.inv_mast_uid
JOIN dbo.p21_view_product_group pg ON pg.product_group_id = il.product_group_id
JOIN dbo.p21_view_location l ON l.location_id = il.location_id
JOIN dbo.p21_view_inventory_supplier s ON s.inv_mast_uid = im.inv_mast_uid
JOIN dbo.inventory_supplier_x_loc sbl ON sbl.inventory_supplier_uid = s.inventory_supplier_uid
AND sbl.location_id = il.location_id
AND sbl.primary_supplier = 'Y'
JOIN dbo.p21_view_supplier ON p21_view_supplier.supplier_id = s.supplier_id
LEFT OUTER JOIN ( SELECT pt.location_id
, il.inv_mast_uid
, il.item_id
, SUM(qty_shipped) qty_sold
, SUM(il.extended_price) sales
FROM dbo.p21_view_invoice_hdr ih
JOIN dbo.p21_view_invoice_line il ON il.invoice_no = ih.invoice_no
JOIN dbo.p21_view_oe_pick_ticket pt ON pt.invoice_no = ih.invoice_no
WHERE DATEDIFF(mm, ih.invoice_date, GETDATE()) <= 6
AND DATEDIFF(mm, ih.invoice_date, GETDATE()) <> 0
GROUP BY pt.location_id
, il.inv_mast_uid
, il.item_id
) AS [6_month]  ON [6_month].inv_mast_uid = il.inv_mast_uid
AND [6_month].location_id = il.location_id
WHERE im.delete_flag = 'N'
AND il.location_id IN (select value from {dashboard}.dbo.fn_CommaSeparatedStringToTable(@locationlist,',') )
AND im.other_charge_item = 'N'
ORDER BY il.item_id
, il.location_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
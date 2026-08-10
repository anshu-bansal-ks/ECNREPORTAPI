SELECT item_id
, item_desc
, supplier_id
, supplier_name
, CAST(ISNULL([100035], 0) AS INT) as NJ
, CAST(ISNULL([100038], 0) AS INT) as FL
, CAST(ISNULL([100105], 0) AS INT) as CA
, CAST(ISNULL([100035], 0) + ISNULL([100038], 0) + ISNULL([100105], 0) AS INT) as [TOTAL]
, price1 
FROM
(
SELECT il.item_id
, im.item_desc
, pt.location_id
, s.supplier_id
, s.supplier_name
, il.qty_shipped
, im.price1
FROM p21_view_invoice_hdr ih
JOIN p21_view_oe_pick_ticket pt ON pt.invoice_no = ih.invoice_no
JOIN p21_view_invoice_line il ON ih.invoice_no = il.invoice_no
JOIN p21_view_inv_mast im ON im.inv_mast_uid = il.inv_mast_uid
JOIN dbo.v_supplier_x_loc sl ON sl.item_id = il.item_id
AND sl.location_id = 100035
JOIN dbo.p21_view_supplier s ON s.supplier_id = sl.supplier_id
WHERE ih.customer_id IN (
SELECT customer_id
FROM {dashboard}.[dbo].[groupcodes]
WHERE [groupcodes].groupcode = @group_code
AND groupcodes.company = @CompId
AND ISNULL(groupcodes.delete_flag, 0) = 0)
AND ih.invoice_date BETWEEN DATEADD(DAY,  - @daysold, CURRENT_TIMESTAMP) AND CURRENT_TIMESTAMP
AND im.other_charge_item = 'N'
GROUP BY pt.location_id
, il.item_id
, s.supplier_id
, s.supplier_name
, im.item_desc
, il.qty_shipped
, im.price1
) SalesQty
PIVOT
(
SUM(qty_shipped)
FOR location_id IN ([100035], [100038], [100105])
) AS PivotTable
ORDER BY PivotTable.supplier_name, item_id
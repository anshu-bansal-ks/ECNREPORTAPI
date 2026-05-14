SELECT invl.item_id
, invl.sales_discount_group_id
, CAST(SUM (invl.qty_shipped) As INT) As qty
, SUM (invl.extended_price) as sales
, SUM (invl.cogs_amount) as cost
, (SUM (invl.extended_price) -SUM (invl.cogs_amount)) as gross_profit
, CASE WHEN SUM (invl.extended_price) =0 THEN 0 ELSE
ROUND (((SUM (invl.extended_price) -SUM (invl.cogs_amount))/ SUM (invl.extended_price)), 2)* 100 END as profit_percent
FROM p21_view_invoice_hdr ih
JOIN p21_view_invoice_line invl ON invl.invoice_no=ih.invoice_no
WHERE ih.invoice_date BETWEEN {dateRange} 
AND invl.sales_discount_group_id IN ( 'CLEAR20', 'CLEAR30', 'CLEAR40', 'CLEAR50', 'CLEAR60', 'CLEAR70', 'FINAL1'
, 'FINAL2' )
GROUP BY invl.item_id
, invl.sales_discount_group_id
ORDER BY invl.item_id
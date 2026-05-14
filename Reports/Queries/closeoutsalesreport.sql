SELECT invl.item_id
, invl.sales_discount_group_id
, CAST(SUM (invl.qty_shipped) AS INT) AS qty
, SUM (invl.extended_price) sales
FROM p21_view_invoice_hdr ih
JOIN p21_view_invoice_line invl ON invl.invoice_no=ih.invoice_no
WHERE ih.invoice_date BETWEEN {dateRange}
AND invl.sales_discount_group_id IN ( 'CLEAR20', 'CLEAR30', 'CLEAR40', 'CLEAR50', 'CLEAR60', 'CLEAR70', 'FINAL1'
, 'FINAL2' )
GROUP BY invl.item_id
, invl.sales_discount_group_id
ORDER BY invl.item_id
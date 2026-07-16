SELECT ih.customer_id
, c.customer_name
, il.item_id
, im.item_desc
, CAST(SUM(il.qty_shipped)AS INT) as qty
, SUM(il.extended_price) as return_amt
FROM p21_view_invoice_hdr ih
JOIN p21_view_oe_hdr oh ON oh.order_no = ih.order_no
JOIN p21_view_invoice_line il ON il.invoice_no = ih.invoice_no
JOIN p21_view_inv_mast im ON im.inv_mast_uid = il.inv_mast_uid
JOIN p21_view_customer c ON c.customer_id = ih.customer_id
WHERE invoice_date BETWEEN {dateRange} 
AND ih.total_amount < 0
AND ih.order_no IS NOT NULL
AND oh.rma_flag = 'Y'
AND c.customer_id = @custId
GROUP BY ih.customer_id
, c.customer_name
, il.item_id
, im.item_desc
ORDER BY customer_name
, il.item_id
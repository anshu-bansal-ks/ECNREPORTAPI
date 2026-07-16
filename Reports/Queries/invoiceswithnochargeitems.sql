SELECT ih.invoice_date
, ih.customer_id
, ih.ship2_name
, ih.invoice_no
, ih.total_amount
, il.item_id
, CAST(il.qty_shipped AS INT ) as qty_shipped
, il.created_by
FROM p21_view_invoice_hdr ih
JOIN dbo.p21_view_invoice_line il ON il.invoice_no = ih.invoice_no
WHERE ih.invoice_date BETWEEN {dateRange}
AND il.extended_price = 0
AND ih.order_no IS NOT NULL
AND ih.rma_flag = 'N'
Order By ih.invoice_date
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
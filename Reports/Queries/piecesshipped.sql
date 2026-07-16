SELECT p21_view_oe_hdr.source_location_id
, p21_view_invoice_hdr.invoice_date
, p21_view_invoice_line.item_id
, CAST(p21_view_invoice_line.qty_shipped AS INT) as qty_shipped
, p21_view_invoice_line.unit_price
FROM p21_view_invoice_hdr p21_view_invoice_hdr (NOLOCK)
, p21_view_invoice_line p21_view_invoice_line (NOLOCK)
, p21_view_oe_hdr p21_view_oe_hdr (NOLOCK)
WHERE p21_view_invoice_line.invoice_no = p21_view_invoice_hdr.invoice_no
AND p21_view_invoice_hdr.order_no = p21_view_oe_hdr.order_no 
AND p21_view_invoice_hdr.invoice_date BETWEEN {dateRange}
AND p21_view_oe_hdr.source_location_id=@locationId
Order By source_location_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
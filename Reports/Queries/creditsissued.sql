SELECT hdr.invoice_no
, ISNULL(hdr.order_no, '') as order_no
, hdr.customer_id
, hdr.bill2_name 
, ISNULL(hdr.po_no, '') as po_no
, line.item_id  
, line.item_desc
, line.extended_price
, CAST(hdr.freight as INT) as freight
, hdr.total_amount
FROM dbo.p21_view_invoice_hdr (NOLOCK) hdr
LEFT OUTER JOIN dbo.p21_view_invoice_line (NOLOCK) line
ON line.invoice_no = hdr.invoice_no
WHERE customer_id = @custId 
AND hdr.invoice_date BETWEEN {dateRange}
AND hdr.total_amount < 0
ORDER BY hdr.invoice_no
, line.item_id
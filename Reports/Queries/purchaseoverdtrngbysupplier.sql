SELECT po_hdr.supplier_id
, supplier.supplier_name
, ROUND(SUM(
CASE WHEN po_line.cancel_flag = 'Y' THEN 
po_line.qty_received * (po_line.unit_price_display / po_line.pricing_unit_size)
WHEN po_line.complete = 'Y' AND po_line.qty_ordered <> po_line.qty_received 
THEN po_line.qty_received * (po_line.unit_price_display / po_line.pricing_unit_size)
ELSE po_line.unit_quantity *  po_line.unit_size * (po_line.unit_price_display / po_line.pricing_unit_size) END), 2) AS purchases
FROM po_hdr (NOLOCK)
INNER JOIN supplier (NOLOCK) ON po_hdr.supplier_id = supplier.supplier_id
INNER JOIN po_line (NOLOCK) ON po_hdr.po_no = po_line.po_no
INNER JOIN location location ON (po_hdr.company_no = location.company_id)
AND (po_hdr.location_id = location.location_id)
WHERE po_hdr.order_date between {dateRange}
AND ( po_line.cancel_flag = 'N'
AND po_line.delete_flag = 'N'
OR po_line.qty_received > 0
)
AND po_hdr.po_type <> 'Q'
GROUP BY po_hdr.supplier_id
, supplier.supplier_name
ORDER BY supplier_name
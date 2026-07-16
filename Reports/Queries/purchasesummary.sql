SELECT s.supplier_id
, s.supplier_name
, order_date
, p21_view_po_hdr.po_no
, p21_view_po_hdr.created_by
, location_name
, SUM(unit_price * qty_ordered) as value
FROM p21_view_po_hdr
JOIN p21_view_po_line ON p21_view_po_line.po_no = p21_view_po_hdr.po_no
JOIN p21_view_location ON p21_view_location.location_id = p21_view_po_hdr.location_id
JOIN p21_view_supplier s ON s.supplier_id = p21_view_po_hdr.supplier_id
JOIN p21_view_inventory_supplier invsup ON (
invsup.inv_mast_uid = p21_view_po_line.inv_mast_uid
AND invsup.supplier_id = p21_view_po_hdr.supplier_id)
WHERE order_date between {dateRange}
AND p21_view_po_line.cancel_flag = 'n'
AND p21_view_po_line.complete = 'n'
AND p21_view_po_line.delete_flag = 'n'
AND p21_view_po_hdr.delete_flag = 'N'
GROUP BY s.supplier_id
, s.supplier_name
, order_date
, p21_view_po_hdr.po_no
, p21_view_po_hdr.created_by
, location_name
ORDER BY s.supplier_name
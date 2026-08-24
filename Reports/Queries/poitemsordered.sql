SELECT p21_view_po_line.po_no
, p21_view_po_hdr.order_date
, p21_view_po_hdr.created_by as buyer
, p21_view_location.location_name
, s.supplier_name
, p21_view_po_line.item_id
, p21_view_po_line.item_description
, p21_view_po_line.qty_ordered
, CASE WHEN unit_of_measure LIKE 'EACH' THEN 'EA' ELSE unit_of_measure END AS unit_of_measure
FROM p21_view_po_hdr
JOIN p21_view_po_line ON p21_view_po_line.po_no = p21_view_po_hdr.po_no
JOIN p21_view_location ON p21_view_location.location_id = p21_view_po_hdr.location_id
JOIN p21_view_supplier s ON s.supplier_id = p21_view_po_hdr.supplier_id
JOIN p21_view_inventory_supplier invsup ON ( invsup.inv_mast_uid = p21_view_po_line.inv_mast_uid
AND invsup.supplier_id = p21_view_po_hdr.supplier_id )
WHERE order_date BETWEEN {dateRange}
AND( @buyer = 'ALL' OR p21_view_po_hdr.created_by = @buyer) 
AND p21_view_po_line.cancel_flag = 'n'
AND p21_view_po_line.complete = 'n'
AND p21_view_po_line.delete_flag = 'n'
ORDER BY order_date
, po_no
, p21_view_po_line.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;

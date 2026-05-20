SELECT hdr.location_id
, p21_view_location.location_name
, hdr.date_created
, hdr.supplier_id
, p21_view_supplier.supplier_name
, hdr.po_no
, external_po_no
, lines.item_id
, ISNULL(invsup.supplier_part_no, '') as supplier_part_no
, lines.item_description
, CAST(lines.qty_ordered as INT ) as qty_ordered
, lines.unit_of_measure as UOM
FROM dbo.p21_view_po_hdr hdr
JOIN p21_view_po_line lines ON hdr.po_no = lines.po_no
JOIN p21_view_supplier (NOLOCK) ON hdr.supplier_id = p21_view_supplier.supplier_id
JOIN dbo.p21_view_inventory_supplier invsup ON (
invsup.item_id = lines.item_id AND invsup.supplier_id = hdr.supplier_id )
JOIN dbo.p21_view_location (NOLOCK) ON p21_view_location.location_id = hdr.location_id
WHERE hdr.external_po_no LIKE '%GALLEY%ORDER%'
AND hdr.date_created BETWEEN {dateRange}
AND (hdr.location_id IN ( @locationId ))
AND invsup.delete_flag = 'N'
AND lines.cancel_flag = 'N'
ORDER BY po_no, invsup.item_id
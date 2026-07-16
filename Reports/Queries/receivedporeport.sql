SELECT p21_view_po_hdr.location_id
, p21_view_location.location_name
, p21_view_inventory_receipts_hdr.date_created
, p21_view_po_hdr.supplier_id
, p21_view_supplier.supplier_name
, p21_view_inventory_receipts_hdr.po_number as po_no
, ISNULL(p21_view_po_hdr.external_po_no, '')as external_po_no
FROM p21_view_inventory_receipts_hdr (NOLOCK)
JOIN p21_view_po_hdr (NOLOCK) ON p21_view_inventory_receipts_hdr.po_number = p21_view_po_hdr.po_no
JOIN p21_view_supplier (NOLOCK) ON p21_view_po_hdr.supplier_id = p21_view_supplier.supplier_id
JOIN dbo.p21_view_location (NOLOCK) ON p21_view_location.location_id = p21_view_po_hdr.location_id
WHERE p21_view_inventory_receipts_hdr.date_created BETWEEN {dateRange}
ORDER BY p21_view_supplier.supplier_name
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
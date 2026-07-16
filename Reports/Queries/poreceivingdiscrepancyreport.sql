SELECT p21_view_inventory_receipts_hdr.date_created
,CAST(p21_view_inventory_receipts_hdr.receipt_number as INT) as receipt_number
,p21_view_po_hdr.po_no
,p21_view_po_hdr.location_id
,p21_view_po_hdr.vendor_id
,p21_view_vendor.vendor_name
,p21_view_inventory_receipt_notepad.topic
,p21_view_inventory_receipt_notepad.note
FROM p21_view_inventory_receipts_hdr
JOIN p21_view_inventory_receipt_notepad ON dbo.p21_view_inventory_receipts_hdr.receipt_number = dbo.p21_view_inventory_receipt_notepad.receipt_number
JOIN p21_view_po_hdr ON dbo.p21_view_inventory_receipt_notepad.po_no = dbo.p21_view_po_hdr.po_no
JOIN p21_view_vendor ON dbo.p21_view_po_hdr.vendor_id = dbo.p21_view_vendor.vendor_id
WHERE (p21_view_inventory_receipt_notepad.note LIKE '*%') 
AND p21_view_inventory_receipts_hdr.date_created BETWEEN {dateRange}
ORDER BY p21_view_inventory_receipts_hdr.date_created 
,p21_view_vendor.vendor_name
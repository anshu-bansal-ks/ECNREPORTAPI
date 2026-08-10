SELECT p21_view_inventory_receipts_hdr.date_created
,CAST(p21_view_inventory_receipts_hdr.receipt_number AS INT ) as receipt_number
,p21_view_po_hdr.po_no     
,p21_view_po_hdr.location_id 
,p21_view_po_hdr.vendor_id 
,p21_view_vendor.vendor_name 
,ISNULL(p21_view_inventory_receipt_notepad.topic, '') as topic
FROM p21_view_inventory_receipts_hdr  (NOLOCK)
JOIN p21_view_po_hdr (NOLOCK) ON p21_view_po_hdr.po_no = p21_view_inventory_receipts_hdr.po_number
JOIN p21_view_vendor (NOLOCK) ON p21_view_vendor.vendor_id = p21_view_po_hdr.vendor_id
LEFT OUTER JOIN p21_view_inventory_receipt_notepad (NOLOCK) ON p21_view_inventory_receipts_hdr.receipt_number = p21_view_inventory_receipt_notepad.receipt_number
WHERE DATEDIFF(dd, p21_view_inventory_receipts_hdr.date_created, GETDATE()) = 0
ORDER BY location_id,vendor_name ,po_no Asc
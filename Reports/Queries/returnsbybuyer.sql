SELECT p21_view_supplier.buyer_id,
p21_view_inventory_return_hdr.supplier_id, 
p21_view_supplier.supplier_name,
p21_view_inventory_return_hdr.date_created, 
p21_view_inventory_return_hdr.return_number AS 'return_no', 
p21_view_inventory_return_hdr.rma_number AS 'rma_no', 
p21_view_inventory_return_hdr.location_id
FROM dbo.p21_view_inventory_return_hdr
p21_view_inventory_return_hdr (NOLOCK), 
dbo.p21_view_supplier p21_view_supplier (NOLOCK)
WHERE p21_view_supplier.supplier_id = p21_view_inventory_return_hdr.supplier_id 
AND (p21_view_inventory_return_hdr.row_status_flag<>976 
And p21_view_inventory_return_hdr.row_status_flag<>975 
And p21_view_inventory_return_hdr.row_status_flag<>974) 
AND ((@status = 'APPROVED' AND p21_view_inventory_return_hdr.rma_number Is Not Null)
OR (@status = 'UNAPPROVED' AND p21_view_inventory_return_hdr.rma_number Is Null))
ORDER BY p21_view_inventory_return_hdr.return_number
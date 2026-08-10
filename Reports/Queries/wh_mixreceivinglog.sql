SELECT p21_view_po_line.item_id
,p21_view_po_line.item_description
,p21_view_po_line.qty_received    
,p21_view_inv_mast.price1         
FROM p21_view_inv_mast JOIN           
p21_view_po_line ON p21_view_po_line.inv_mast_uid = p21_view_inv_mast.inv_mast_uid JOIN 
p21_view_po_hdr ON p21_view_po_hdr.po_no = p21_view_po_line.po_no
WHERE p21_view_po_line.qty_received > $0 
AND DATEDIFF(dd,p21_view_po_line.received_date, GETDATE()) = 0
AND p21_view_po_hdr.branch_id = '01'
AND p21_view_inv_mast.default_sales_discount_group = 'dvdmix'
ORDER BY p21_view_inv_mast.item_id
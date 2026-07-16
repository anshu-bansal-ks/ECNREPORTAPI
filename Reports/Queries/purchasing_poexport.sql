SELECT p21_view_po_line.po_no
, p21_view_location.location_name
, p21_view_po_line.item_id       
, ISNULL(invsup.supplier_part_no, '') as supplier_part_no
, p21_view_po_line.item_description         
, CASE WHEN unit_of_measure LIKE 'EACH' THEN 'EA' ELSE unit_of_measure END AS 'unit_of_measure'                  
, CAST(p21_view_po_line.qty_ordered AS INT) as qty_ordered                              
FROM p21_view_po_hdr                             
JOIN p21_view_po_line ON p21_view_po_line.po_no = p21_view_po_hdr.po_no
JOIN p21_view_location ON p21_view_location.location_id = p21_view_po_hdr.location_id
JOIN p21_view_inventory_supplier invsup ON ( invsup.inv_mast_uid = p21_view_po_line.inv_mast_uid
AND invsup.supplier_id = p21_view_po_hdr.supplier_id )
WHERE p21_view_po_line.po_no = @pono   
AND p21_view_po_line.cancel_flag = 'n'   
AND p21_view_po_line.complete = 'n'      
AND p21_view_po_line.delete_flag = 'n'   
ORDER BY p21_view_po_line.item_id 
SELECT p21_view_inventory_receipts_hdr.receipt_number as receipt_no
, p21_view_inventory_receipts_hdr.po_number as po_no     
, p21_view_inventory_receipts_line.item_id      
, p21_view_inv_mast.item_desc                   
, CAST(p21_view_inventory_receipts_line.qty_received AS INT ) as qty_received
, p21_view_inv_loc.primary_bin                  
, p21_view_inventory_receipts_hdr.date_created  
, CAST(p21_view_inv_loc.qty_on_hand AS INT) as qty_on_hand                
, CAST(p21_view_inv_loc.qty_backordered AS INT ) as qty_backordered                        
FROM p21_view_inventory_receipts_hdr (NOLOCK)
JOIN p21_view_inventory_receipts_line (NOLOCK) ON p21_view_inventory_receipts_line.receipt_number = p21_view_inventory_receipts_hdr.receipt_number
LEFT OUTER JOIN dbo.p21_view_po_hdr(NOLOCK) ON p21_view_po_hdr.po_no = p21_view_inventory_receipts_hdr.po_number 
JOIN p21_view_inv_mast(NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inventory_receipts_line.inv_mast_uid 
LEFT OUTER JOIN dbo.transfer_shipment_hdr  (NOLOCK) ON transfer_shipment_hdr.transfer_shipment_no = p21_view_inventory_receipts_hdr.po_number
LEFT OUTER JOIN dbo.transfer_hdr(NOLOCK) ON transfer_hdr.transfer_no = transfer_shipment_hdr.transfer_no         
JOIN p21_view_inv_loc(NOLOCK) ON ( p21_view_inv_loc.inv_mast_uid = p21_view_inv_mast.inv_mast_uid )              
WHERE p21_view_inventory_receipts_hdr.po_number = @pono
AND p21_view_inventory_receipts_line.qty_received > $0
AND p21_view_inv_loc.location_id = COALESCE(p21_view_po_hdr.location_id,transfer_hdr.to_location_id, 999999)
ORDER BY p21_view_inventory_receipts_line.item_id
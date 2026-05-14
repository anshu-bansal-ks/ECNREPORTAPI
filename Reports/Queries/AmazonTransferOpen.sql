SELECT p21_view_transfer_hdr.transfer_no,
p21_view_transfer_hdr.from_location_id,
p21_view_transfer_hdr.to_location_id,
p21_view_transfer_hdr.delete_flag,
p21_view_transfer_hdr.complete_flag,
p21_view_transfer_hdr.printed_date,
p21_view_transfer_line.item_id,
p21_view_transfer_line.qty_to_transfer,
p21_view_transfer_line.qty_transferred,
p21_view_transfer_line.qty_received
FROM p21_view_transfer_hdr 
JOIN p21_view_transfer_line (NOLOCK) ON p21_view_transfer_hdr.transfer_no = p21_view_transfer_line.transfer_no
WHERE p21_view_transfer_hdr.to_location_id = @locationId
AND (p21_view_transfer_hdr.delete_flag = 'n')
AND (p21_view_transfer_hdr.complete_flag = 'n')
AND (p21_view_transfer_line.qty_to_transfer <> p21_view_transfer_line.qty_received)
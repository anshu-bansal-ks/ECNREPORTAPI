SELECT p21_view_transfer_hdr.transfer_no,
p21_view_transfer_hdr.from_location_id,
p21_view_transfer_hdr.to_location_id,
p21_view_transfer_hdr.delete_flag,
p21_view_transfer_hdr.complete_flag,
p21_view_transfer_hdr.printed_date,
p21_view_transfer_line.item_id,
CAST(p21_view_transfer_line.qty_to_transfer AS INT) as qty_to_transfer,
CAST(p21_view_transfer_line.qty_transferred AS INT) as qty_transferred,
CAST(p21_view_transfer_line.qty_received AS INT) as qty_received,
p21_view_transfer_line.created_by
FROM p21_view_transfer_hdr p21_view_transfer_hdr,
p21_view_transfer_line p21_view_transfer_line
WHERE p21_view_transfer_hdr.transfer_no = p21_view_transfer_line.transfer_no
AND ((p21_view_transfer_hdr.delete_flag='n') AND (p21_view_transfer_hdr.complete_flag='n')
AND (p21_view_transfer_line.qty_to_transfer<>p21_view_transfer_line.qty_received));
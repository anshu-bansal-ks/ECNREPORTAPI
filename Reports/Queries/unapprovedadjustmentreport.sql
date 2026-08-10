SELECT p21_view_inv_adj_hdr.location_id
, p21_view_inv_adj_hdr.adjustment_number
, p21_view_inv_adj_line.item_id
, p21_view_inv_mast.item_desc
, cast(p21_view_inv_adj_line.quantity as int) AS quantity
, p21_view_inv_adj_hdr.date_created
, p21_view_inv_adj_hdr.last_maintained_by
, p21_view_inv_adj_line.cost
, p21_view_reason.reason
FROM p21_view_inv_adj_hdr
JOIN p21_view_inv_adj_line ON p21_view_inv_adj_hdr.adjustment_number = p21_view_inv_adj_line.adjustment_number
JOIN p21_view_inv_mast ON p21_view_inv_adj_line.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
JOIN dbo.p21_view_reason ON p21_view_inv_adj_hdr.reason_id = p21_view_reason.id
WHERE p21_view_inv_adj_hdr.approved = 'n'
AND p21_view_inv_adj_hdr.delete_flag = 'n'
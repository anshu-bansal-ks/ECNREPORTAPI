SELECT p21_view_oe_hdr.order_no
, p21_view_oe_line.item_id
, p21_view_inv_mast.item_desc
, p21_view_inv_loc.primary_bin
, CAST(p21_view_oe_line.qty_ordered AS INT) as qty_ordered
FROM p21_view_oe_hdr
JOIN p21_view_oe_line ON p21_view_oe_hdr.oe_hdr_uid = p21_view_oe_line.oe_hdr_uid
JOIN p21_view_inv_mast ON p21_view_inv_mast.inv_mast_uid = p21_view_oe_line.inv_mast_uid
JOIN p21_view_inv_loc ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
AND p21_view_inv_loc.location_id = p21_view_oe_hdr.source_location_id
WHERE p21_view_oe_hdr.order_no = @ordernum
ORDER BY primary_bin
SELECT p21_view_oe_line.line_no
, p21_view_oe_hdr.order_no
, p21_view_oe_hdr.customer_id
, p21_view_oe_hdr.ship2_name
, p21_view_oe_line.item_id
, p21_view_inv_mast.item_desc
, p21_view_oe_line.qty_ordered
, ISNULL(p21_view_inv_mast.weight, 0) as [weight(Lb)]
, p21_view_oe_line.qty_ordered * ISNULL(p21_view_inv_mast.weight, 0) as [total_weight(Lb)]
,((p21_view_oe_line.qty_ordered * ISNULL(p21_view_inv_mast.weight, 0)) / 2.2) as [total_weight(Kg)]
FROM p21_view_oe_hdr (NOLOCK)
JOIN p21_view_oe_line ON p21_view_oe_line.order_no = p21_view_oe_hdr.order_no
JOIN p21_view_inv_mast ON p21_view_inv_mast.inv_mast_uid = p21_view_oe_line.inv_mast_uid
WHERE p21_view_oe_hdr.order_no = @ordernum
SELECT oe_hdr.order_no
,order_date
,oe_hdr.po_no
,ship2_name
,oe_hdr.created_by
,oe_hdr.cancel_flag
,projected_order
,oe_hdr.delete_flag
,oe_line.supplier_id
,supplier.supplier_name
,SUM(oe_line.qty_allocated * oe_line.unit_price) as value
FROM oe_hdr (NOLOCK)
JOIN oe_line (NOLOCK) ON oe_line.order_no = oe_hdr.order_no
JOIN dbo.supplier (NOLOCK) ON supplier.supplier_id = oe_line.supplier_id
WHERE oe_hdr.order_no = @ordernum
GROUP BY oe_hdr.order_no
,order_date
,oe_hdr.po_no
,ship2_name
,oe_hdr.created_by
,oe_hdr.cancel_flag
,projected_order
,oe_hdr.delete_flag
,projected_order
,oe_hdr.delete_flag
,oe_line.supplier_id
,supplier.supplier_name
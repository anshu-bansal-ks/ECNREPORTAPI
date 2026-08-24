SELECT oe_hdr.order_no
, order_date
, oe_hdr.po_no
, ship2_name
, oe_hdr.created_by
, oe_hdr.cancel_flag
, projected_order
, oe_hdr.delete_flag
, oe_line.supplier_id
, supplier.supplier_name
, SUM(IIF(ss.supplier_id IS NOT NULL, oe_line.qty_allocated * oe_line.unit_price, 0)) as SHOW_SALES
, discount as discount_percent
, SUM(IIF(ss.supplier_id IS NOT NULL AND ss.discount > 0, -oe_line.qty_allocated * oe_line.unit_price * ss.discount/100, 0)) as discount_amt
, SUM(IIF(ss.supplier_id IS NULL, oe_line.qty_allocated * oe_line.unit_price, 0)) as NON_SHOW_SALES 
FROM oe_hdr (NOLOCK)
JOIN oe_line (NOLOCK) ON oe_line.order_no = oe_hdr.order_no
JOIN dbo.supplier (NOLOCK) ON supplier.supplier_id = oe_line.supplier_id
Left join {dashboard}.dbo.ShowsSupplier ss on ss.supplier_id = supplier.supplier_id And ShowId=@showId
WHERE oe_hdr.order_no = @ordernum
AND ( @AllPO = 'true' OR @pono = '' OR oe_hdr.po_no LIKE '%' + @pono + '%')
GROUP BY oe_hdr.order_no
, order_date
, oe_hdr.po_no
, ship2_name
, oe_hdr.created_by
, oe_hdr.cancel_flag
, projected_order
, oe_hdr.delete_flag
, projected_order
, oe_hdr.delete_flag
, oe_line.supplier_id
, supplier.supplier_name
, ss.discount
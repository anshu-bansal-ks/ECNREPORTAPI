SELECT oe_hdr.order_no
,order_date
,oe_hdr.po_no
,ship2_name
,inv_mast.item_id
,inv_mast.item_desc
,oe_hdr.created_by
,oe_hdr.cancel_flag
,projected_order
,oe_hdr.delete_flag
,oe_line.supplier_id
,supplier.supplier_name
,SUM(IIF(p.ItemId IS NOT NULL, oe_line.qty_allocated * oe_line.unit_price, 0)) PROMO_SALES
,discount as discount_perc
,SUM(IIF(p.ItemId IS NOT NULL AND p.discount > 0, -oe_line.qty_allocated * oe_line.unit_price * p.discount/100, 0)) discount_Sales
,SUM(IIF(p.ItemId IS NULL, oe_line.qty_allocated * oe_line.unit_price, 0)) NON_PROMO_SALES 
FROM oe_hdr (NOLOCK)
JOIN oe_line (NOLOCK) ON oe_line.order_no = oe_hdr.order_no
JOIN dbo.supplier (NOLOCK) ON supplier.supplier_id = oe_line.supplier_id
JOIN inv_mast WITH (NOLOCK) ON inv_mast.inv_mast_uid = oe_line.inv_mast_uid
LEFT join {dashboard}.dbo.PromosItems p on p.ItemId = inv_mast.item_id And p.promoid=@PromoId
WHERE oe_hdr.order_no = @ordernum
AND ( @AllPO = 'true' OR @pono = '' OR oe_hdr.po_no LIKE '%' + @pono + '%')
GROUP BY oe_hdr.order_no
,order_date
,oe_hdr.po_no
,ship2_name
,inv_mast.item_id
,inv_mast.item_desc
,oe_hdr.created_by
,oe_hdr.cancel_flag
,projected_order
,oe_hdr.delete_flag
,projected_order
,oe_hdr.delete_flag
,oe_line.supplier_id
,supplier.supplier_name
,p.discount
SELECT p21_view_oe_line.line_no
, p21_view_oe_hdr.order_date
, p21_view_oe_hdr.order_no
, p21_view_oe_hdr.customer_id
, p21_view_oe_hdr.ship2_name
, p21_view_oe_hdr.po_no
, p21_view_oe_line.item_id
, p21_view_inv_mast.item_desc
, Cast(p21_view_oe_line.qty_invoiced AS INT) as qty_invoiced
, ISNULL(p21_view_inv_mast.weight, 0) as weight
, ISNULL(inv_mast_ud.country_of_origin, '') as country
, ISNULL(inv_mast_ud.class_code, '') as class_code
, ISNULL(inv_mast_ud.material1, '') as material1
, ISNULL(inv_mast_ud.material2, '') as material2
, ISNULL(inv_mast_ud.battery, '') as battery
, ISNULL(inv_mast_ud.battery_location, '') as battery_location
FROM p21_view_oe_hdr
JOIN p21_view_oe_line (NOLOCK) ON p21_view_oe_hdr.oe_hdr_uid = p21_view_oe_line.oe_hdr_uid
JOIN p21_view_inv_mast (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_oe_line.inv_mast_uid
JOIN dbo.inv_mast_ud (NOLOCK) ON inv_mast_ud.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
JOIN customer with (nolock) ON customer.customer_id = p21_view_oe_hdr.customer_id
WHERE p21_view_oe_hdr.order_no = @ordernum
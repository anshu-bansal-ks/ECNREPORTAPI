SELECT CASE WHEN c.class_1id = 'ADS' THEN 'ADS' ELSE 'B2B' END AS Order_Type
, l.location_name
, COUNT(DISTINCT pick_ticket_no) as Pick_Count
, SUM(p21_view_oe_line.qty_on_pick_tickets * p21_view_oe_line.unit_price) as open_value
, SUM(p21_view_oe_line.qty_on_pick_tickets * p21_view_oe_line.unit_price) / COUNT(DISTINCT pick_ticket_no) as avg_amount
, MIN(print_date) as oldest
, MAX(print_date) as newest
FROM p21_view_oe_pick_ticket
JOIN p21_view_oe_hdr (NOLOCK) ON p21_view_oe_pick_ticket.order_no = p21_view_oe_hdr.order_no
JOIN p21_view_oe_line (NOLOCK) ON p21_view_oe_line.oe_hdr_uid = p21_view_oe_hdr.oe_hdr_uid
JOIN location AS l (NOLOCK) ON l.location_id = p21_view_oe_pick_ticket.location_id
JOIN p21_view_customer c ON c.customer_id = p21_view_oe_hdr.customer_id
WHERE p21_view_oe_pick_ticket.delete_flag = 'N'
AND p21_view_oe_pick_ticket.delete_flag = 'n'
AND p21_view_oe_pick_ticket.print_date > DATEADD(MONTH, -2, GETDATE())
AND p21_view_oe_hdr.order_date > DATEADD(MONTH, -2, GETDATE())
AND p21_view_oe_pick_ticket.ship_date IS NULL
GROUP BY p21_view_oe_hdr.company_id
, l.location_name
, CASE WHEN c.class_1id = 'ADS' THEN 'ADS' ELSE 'B2B' END
ORDER BY l.location_name, Order_Type
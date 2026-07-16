SELECT CASE WHEN ship2_name LIKE 'ADS -%' THEN 'ADS' ELSE 'B2B' END Order_Type                     
, p21_view_location.location_name 
, COUNT(pick_ticket_no) as Pick_Count 
, MIN(p21_view_oe_pick_ticket.print_date) as Oldest
, MAX(p21_view_oe_pick_ticket.print_date) as Newest
FROM dbo.p21_view_oe_pick_ticket              
JOIN p21_view_oe_hdr ON p21_view_oe_hdr.order_no = p21_view_oe_pick_ticket.order_no
JOIN p21_view_location ON p21_view_location.location_id = p21_view_oe_pick_ticket.location_id
WHERE (p21_view_oe_pick_ticket.delete_flag = 'N' AND p21_view_oe_pick_ticket.delete_flag = 'n' )
AND (p21_view_oe_pick_ticket.ship_date IS NULL)
GROUP BY CASE
WHEN ship2_name LIKE 'ADS -%' THEN 'ADS' ELSE 'B2B' END          
, p21_view_location.location_name             
ORDER BY location_name, Order_Type DESC;
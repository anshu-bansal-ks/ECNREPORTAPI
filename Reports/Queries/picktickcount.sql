SELECT pv.date_printed 
,ISNULL(pv.[6], 0) as [6:00]  
,ISNULL(pv.[7], 0) as [7:00]  
,ISNULL(pv.[8], 0) as [8:00]  
,ISNULL(pv.[9], 0) as [9:00]  
,ISNULL(pv.[10], 0) as [10:00]
,ISNULL(pv.[11], 0) as [11:00]
,ISNULL(pv.[12], 0) as [12:00]
,ISNULL(pv.[13], 0) as [13:00]
,ISNULL(pv.[14], 0) as [14:00]
,ISNULL(pv.[15], 0) as [15:00]
,ISNULL(pv.[16], 0) as [16:00]
,ISNULL(pv.[17], 0) as [17:00]
,ISNULL(pv.[18], 0) as [18:00]
,ISNULL(pv.[19], 0) as [19:00]
,ISNULL(pv.[20], 0) as [20:00] 
,(ISNULL(pv.[6], 0) + ISNULL(pv.[7], 0) + ISNULL(pv.[8], 0) + ISNULL(pv.[9], 0)+ ISNULL(pv.[10], 0)
+ ISNULL(pv.[11], 0) + ISNULL(pv.[12], 0) + ISNULL(pv.[13], 0) + ISNULL(pv.[14], 0) + ISNULL(pv.[15], 0)
+ ISNULL(pv.[16], 0) + ISNULL(pv.[17], 0) + ISNULL(pv.[18], 0) + ISNULL(pv.[19], 0) + ISNULL(pv.[20], 0) ) as Total
FROM ( SELECT CONVERT(DATE, p21_view_oe_pick_ticket.print_date) date_printed    
,CASE WHEN DATEPART(HOUR,
p21_view_oe_pick_ticket.print_date) < 7 THEN 6      
WHEN DATEPART(HOUR, p21_view_oe_pick_ticket.print_date) > 19 THEN 20 ELSE DATEPART(HOUR,
p21_view_oe_pick_ticket.print_date) END hour_printed 
,ISNULL(COUNT(DISTINCT p21_view_oe_pick_ticket.pick_ticket_no),0) As 'Tickets'     
FROM p21_view_oe_pick_ticket 
JOIN p21_view_oe_pick_ticket_detail ON p21_view_oe_pick_ticket_detail.pick_ticket_no = p21_view_oe_pick_ticket.pick_ticket_no
JOIN p21_view_oe_hdr ON p21_view_oe_hdr.order_no = p21_view_oe_pick_ticket.order_no
WHERE p21_view_oe_pick_ticket.print_date BETWEEN {dateRange}  
AND p21_view_oe_pick_ticket.delete_flag = 'N'
AND p21_view_oe_hdr.source_location_id = @locationId 
AND p21_view_oe_hdr.ship2_name NOT LIKE 'ADS%'
AND rma_flag = 'N'   
GROUP BY CONVERT(DATE, p21_view_oe_pick_ticket.print_date) ,DATEPART(HOUR, p21_view_oe_pick_ticket.print_date)) AS pivdata PIVOT    
(SUM(Tickets) FOR hour_printed IN ([6], [7], [8], [9], [10], [11], [12], [13],[14],[15], [16], [17], [18], [19], [20])) AS pv
ORDER BY pv.date_printed ASC
SELECT oh.source_location_id as location_id
, l.location_name
, SUM(ih.total_amount - ih.freight) as sales
, SUM(ih.freight) as freight
, SUM(ih.total_amount) as total_amount
FROM p21_view_invoice_hdr ih ( NOLOCK )
JOIN dbo.p21_view_oe_hdr oh (NOLOCK) ON oh.order_no = ih.order_no
JOIN dbo.p21_view_location l (NOLOCK) ON l.location_id = oh.source_location_id
JOIN dbo.p21_view_customer cust ON cust.customer_id = ih.customer_id
WHERE ih.invoice_date BETWEEN {dateRange} 
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND cust.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND cust.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (cust.class_1id = 'B2B' OR cust.class_1id NOT IN ('ADS', 'KIOSK') OR cust.class_1id IS NULL))
)   
GROUP BY oh.source_location_id
, l.location_name
ORDER BY l.location_name
SELECT p21_view_oe_hdr.source_location_id as location_id
, p21_view_location.location_name
, CAST(SUM(p21_view_invoice_line.qty_shipped ) as int) AS 'qty_shipped'
, SUM(p21_view_invoice_line.extended_price) AS 'total_amount'
FROM ccxg.dbo.p21_view_invoice_hdr (NOLOCK)
JOIN ccxg.dbo.p21_view_invoice_line (NOLOCK) ON p21_view_invoice_hdr.invoice_no = p21_view_invoice_line.invoice_no
JOIN ccxg.dbo.p21_view_oe_hdr (NOLOCK) ON p21_view_oe_hdr.order_no = p21_view_invoice_hdr.order_no
JOIN ccxg.dbo.p21_view_location (NOLOCK) ON p21_view_location.location_id = p21_view_oe_hdr.source_location_id
JOIN customer (NOLOCK) cust ON cust.customer_id = ccxg.dbo.p21_view_invoice_hdr.customer_id 
WHERE p21_view_invoice_hdr.invoice_date BETWEEN {dateRange}
AND (@custclass = 'ALL' OR (@custclass = 'ADS' AND cust.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND cust.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (cust.class_1id = 'B2B' OR cust.class_1id NOT IN ('ADS', 'KIOSK') OR cust.class_1id IS NULL))
)
AND p21_view_invoice_hdr.total_amount > 0
GROUP BY p21_view_oe_hdr.source_location_id
, p21_view_location.location_name
UNION
SELECT p21_view_oe_hdr.source_location_id as location_id
, p21_view_location.location_name
, CAST(SUM(p21_view_invoice_line.qty_shipped ) as int) AS 'qty_shipped'
, SUM(p21_view_invoice_line.extended_price) AS 'total_amount'
FROM ccecn.dbo.p21_view_invoice_hdr (NOLOCK)
JOIN ccecn.dbo.p21_view_invoice_line (NOLOCK) ON p21_view_invoice_hdr.invoice_no = dbo.p21_view_invoice_line.invoice_no
JOIN ccecn.dbo.p21_view_oe_hdr (NOLOCK) ON p21_view_oe_hdr.order_no = p21_view_invoice_hdr.order_no
JOIN ccecn.dbo.p21_view_location (NOLOCK) ON p21_view_location.location_id = p21_view_oe_hdr.source_location_id
JOIN customer (NOLOCK) cust ON cust.customer_id = ccecn.dbo.p21_view_invoice_hdr.customer_id 
WHERE p21_view_invoice_hdr.invoice_date BETWEEN {dateRange}
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND cust.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND cust.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (cust.class_1id = 'B2B' OR cust.class_1id NOT IN ('ADS', 'KIOSK') OR cust.class_1id IS NULL))
)
AND p21_view_invoice_hdr.total_amount > 0
GROUP BY p21_view_oe_hdr.source_location_id
, p21_view_location.location_name
ORDER BY location_name
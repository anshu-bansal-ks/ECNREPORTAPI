SELECT cust.customer_id
,cust.customer_name
,ISNULL(totalship, 0) total_ship
,ISNULL(totalhandling, 0) total_handling
,ISNULL(totalfeed, 0) total_feed
,ISNULL(totalother, 0) total_other
,SUM(il.extended_price) total_merch
,SUM(il.cogs_amount) total_cost
,ISNULL(totalship, 0) + ISNULL(totalhandling, 0) + ISNULL(totalfeed, 0)+ ISNULL(totalother, 0) + SUM(il.extended_price) grand_total
,CASE WHEN SUM(il.extended_price) = 0 THEN 0
 ELSE ROUND(( SUM(il.extended_price) - SUM(il.cogs_amount) ) / SUM(il.extended_price) * 100, 2)
END profit_percent
FROM dbo.invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN customer cust ( NOLOCK ) ON ih.customer_id = cust.customer_id
JOIN dbo.invoice_hdr_salesrep ihs ( NOLOCK ) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c ( NOLOCK ) ON c.id = ihs.salesrep_id
LEFT OUTER JOIN ( SELECT CAST(ih.customer_id AS DECIMAL) customer_id
,SUM(il.extended_price) totalship
FROM dbo.invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
WHERE ih.invoice_date between {dateRange} 
AND ship2_name LIKE 'ADS -%'
AND item_id LIKE 'ADS -%SHIP%'
GROUP BY ih.customer_id
) AS SHIPCHRG ON SHIPCHRG.customer_id = ih.customer_id
LEFT OUTER JOIN ( SELECT CAST(ih.customer_id AS DECIMAL) customer_id
,SUM(il.extended_price) totalhandling
FROM dbo.invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
WHERE ih.invoice_date between {dateRange} 
AND ship2_name LIKE 'ADS -%'
AND item_id LIKE 'ADS -%HANDLING%'
GROUP BY ih.customer_id
) AS HNDLCHG ON HNDLCHG.customer_id = ih.customer_id
LEFT OUTER JOIN ( SELECT CAST(ih.customer_id AS DECIMAL) customer_id
,SUM(il.extended_price) totalfeed
FROM dbo.invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
WHERE ih.invoice_date between {dateRange} 
AND ship2_name LIKE 'ADS -%'
AND item_id LIKE 'ADS -%FEED%'
GROUP BY ih.customer_id
) AS FEEDCHRG ON FEEDCHRG.customer_id = ih.customer_id
LEFT OUTER JOIN ( SELECT CAST(ih.customer_id AS DECIMAL) customer_id
,SUM(il.extended_price) totalother
FROM dbo.invoice_hdr ih ( NOLOCK )
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
WHERE ih.invoice_date between {dateRange}
AND ship2_name LIKE 'ADS -%'
AND ( item_id LIKE 'ADS -%'
AND item_id NOT LIKE 'ADS -%FEED%'
AND item_id NOT LIKE 'ADS -%HANDLING%'
AND item_id NOT LIKE 'ADS -%SHIP%'
)
GROUP BY ih.customer_id
) AS OTHERCHRG ON OTHERCHRG.customer_id = ih.customer_id
WHERE ih.invoice_date between {dateRange}
AND customer_name LIKE 'ADS -%'
AND item_id NOT LIKE 'ADS -%'
GROUP BY cust.customer_id
,cust.customer_name
,totalship
,totalhandling
,totalfeed
,totalother
ORDER BY cust.customer_name
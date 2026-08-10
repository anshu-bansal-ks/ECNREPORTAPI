SELECT salesrep_id
, Rep
, ISNULL (B2B, 0) as B2B
, ISNULL (ADS, 0) as ADS
, ISNULL (KIOSK, 0) as KIOSK
, ISNULL (B2B, 0) + ISNULL (ADS, 0) + ISNULL (KIOSK, 0) as SALES
FROM
( SELECT ihs.salesrep_id
, c.first_name + ' ' + c.last_name Rep
, CASE WHEN cust.class_1id = 'ADS' THEN 'ADS' WHEN cust.class_1id = 'KIOSK' THEN'KIOSK' ELSE 'B2B'
END [Type]
, SUM (il.extended_price) sls
FROM invoice_hdr ih (NOLOCK)
JOIN customer (NOLOCK) cust ON cust.customer_id = ih.customer_id
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
GROUP BY ihs.salesrep_id
, c.first_name + ' ' + c.last_name
, CASE WHEN cust.class_1id = 'ADS' THEN
'ADS' WHEN cust.class_1id = 'KIOSK' THEN 'KIOSK' ELSE 'B2B' END
) SalesTotals
PIVOT
(
SUM(sls)
FOR Type IN ([B2B], [ADS], [KIOSK])
) AS PIVOTTABLE
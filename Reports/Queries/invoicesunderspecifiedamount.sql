SELECT ih.invoice_no
, ih.invoice_date
, ih.po_no
, ih.bill2_name
, rep
, ih.total_amount
, ih.freight
FROM p21_view_invoice_hdr ih
JOIN DA_Rep r ON r.customer_id=ih.customer_id
JOIN customer (NOLOCK) cust ON cust.customer_id = ih.customer_id
WHERE invoice_date BETWEEN {dateRange}
AND ih.total_amount-ih.freight>0
AND ih.total_amount-ih.freight<=@freight
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND cust.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND cust.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (cust.class_1id = 'B2B' OR cust.class_1id NOT IN ('ADS', 'KIOSK') OR cust.class_1id IS NULL))
)
AND ih.bill2_name NOT LIKE '%EMPLOYEE%'
AND ih.bill2_name NOT LIKE '%ONE%TIME%'
Order By ih.bill2_name
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
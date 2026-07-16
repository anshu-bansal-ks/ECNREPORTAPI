SELECT c.customer_id
, c.customer_name
, t.terms_desc
, DA_Rep.rep
, a.[Current]
, a.[31_60] As [30_to_60]
, a.[61_90] as [60_to_90]
, a.Over90 As Over_90
, a.Total_Due
FROM p21_view_customer (NOLOCK) c
JOIN dbo.v_aging (NOLOCK) a ON a.customer_id = c.customer_id
JOIN DA_Rep (NOLOCK) ON DA_Rep.customer_id = a.customer_id
JOIN p21_view_terms (NOLOCK) t ON t.terms_id=c.terms_id
WHERE 
(@custclass = 'ALL' OR (@custclass = 'ADS' AND c.class_1id = 'ADS')
OR (@custclass = 'KIOSK' AND c.class_1id = 'KIOSK')
OR (@custclass = 'B2B' AND (c.class_1id = 'B2B' OR c.class_1id NOT IN ('ADS', 'KIOSK') OR c.class_1id IS NULL))
) AND 
( @repId = 'ALL' OR DA_Rep.salesrep_id = @repId) AND
a.Total_Due > 0
ORDER BY c.customer_name
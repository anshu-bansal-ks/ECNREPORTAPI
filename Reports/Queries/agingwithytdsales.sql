SELECT c.customer_id
, c.customer_name
, r.rep
, t.terms_desc
, c.last_check_date as last_pmt
, ISNULL(c.last_check_amount,0) as last_pmt_amount
, ytd.ytd_sales
, ytd.ly_sales
, a.[Current] 
, a.[31_60] as [31_to_60]
, a.[61_90] as [61_to_90]
, a.Over90 
, a.Total_Due
FROM p21_view_customer (NOLOCK) c
JOIN dbo.v_aging (NOLOCK) a ON a.customer_id=c.customer_id
JOIN DA_Rep (NOLOCK) r ON r.customer_id=c.customer_id
JOIN dbo.p21_view_terms (NOLOCK) t ON t.terms_id=c.terms_id
LEFT OUTER JOIN dbo.da_ytd_static (NOLOCK) ytd ON ytd.customer_id=c.customer_id
WHERE a.Total_Due<>0
And ( @repId = 'ALL' OR r.salesrep_id = @repId) 
ORDER BY rep
, c.customer_name
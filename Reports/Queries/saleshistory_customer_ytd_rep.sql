SELECT c.customer_id
,c.customer_name
,(rep1.rep + ' - ' + rep1.salesrep_id) as rep
,ISNULL(ytd.lytd_sales, 0) as lytd 
,ISNULL(ytd.ytd_sales, 0) as ytd   
,ISNULL(ytd.ytd_sales, 0) - ISNULL(ytd.lytd_sales, 0) as Change
,ISNULL((ISNULL(ytd.ytd_sales, 0) - ISNULL(ytd.lytd_sales, 0))/NULLIF(ytd.lytd_sales,0),0)*100 'percent'
,stats.FirstSl as First_Sl   
,stats.LastSl as Last_Sl    
,stats.LastSlDays as Last_Sl_Days
FROM customer (NOLOCK) c
JOIN dbo.da_ytd_static (NOLOCK) ytd ON ytd.customer_id = c.customer_id
LEFT OUTER JOIN dbo.DA_Cust_Stats_Static (NOLOCK) stats ON stats.customer_id = c.customer_id
LEFT OUTER  JOIN dbo.DA_Rep (NOLOCK) rep1 ON rep1.customer_id = c.customer_id
WHERE (@repId = 'ALL' OR rep1.salesrep_id = @repId )
And (@custclass = 'ALL' OR (@custclass = 'ADS' AND c.class_1id = 'ADS')
OR (@custclass = 'B2B' AND (c.class_1id = 'ADS' OR c.class_1id IS NULL))
)
AND ISNULL(ytd.ytd_sales, 0) + ISNULL(ytd.ly_sales, 0) > 0
ORDER BY ISNULL(ytd.ytd_sales, 0) - ISNULL(ytd.lytd_sales, 0)
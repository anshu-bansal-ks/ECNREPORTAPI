SELECT (rep + ' - ' + rep.salesrep_id) as salesrep
,SUM(ytd.lytd_sales) as lytd
,SUM(ytd.ytd_sales) as ytd  
,SUM(ytd.ytd_sales) - SUM(ytd.lytd_sales) as Change
,ISNULL(( SUM(ytd.ytd_sales) - SUM(ytd.lytd_sales) ) / NULLIF(SUM(ytd.lytd_sales), 0),0)*100 'percent'
FROM customer (NOLOCK) c 
JOIN dbo.da_ytd_margin_static (NOLOCK) ytd ON ytd.customer_id = c.customer_id
LEFT OUTER JOIN dbo.DA_Cust_Stats_Static (NOLOCK) stats ON stats.customer_id = c.customer_id
LEFT OUTER JOIN dbo.DA_Rep (NOLOCK) rep ON rep.customer_id = c.customer_id
GROUP BY rep.salesrep_id
,rep
HAVING  SUM(ytd.ytd_sales) + SUM(ytd.lytd_sales) > 0  
ORDER BY rep
SELECT {topsub}
       c.customer_name,
       ISNULL(ytd.lytd_sales,0) AS lytd,
       ISNULL(ytd.lytd_sales,0)-ISNULL(ytd.lytd_cost,0) AS lytd_gp,
       ISNULL(ytd.lytd_margin,0) AS lytd_margin,
       ISNULL(ytd.ytd_sales,0) AS ytd,
       ISNULL(ytd.ytd_sales,0)-ISNULL(ytd.ytd_cost,0) AS ytd_gp,
       ISNULL(ytd.ytd_margin,0) AS ytd_margin,
       ISNULL(ytd.ytd_sales,0)-ISNULL(ytd.lytd_sales,0) AS Sls_Change,
       CASE
           WHEN ISNULL(ytd.lytd_sales,0)=0 THEN 1
           ELSE ((ISNULL(ytd.ytd_sales,0)-ISNULL(ytd.lytd_sales,0))/ytd.lytd_sales)*100
       END AS Change_percent,
       rep1.rep
FROM customer (NOLOCK) c
JOIN DA_YTD_MARGIN ytd (NOLOCK)
    ON ytd.customer_id = c.customer_id
LEFT JOIN dbo.DA_Cust_Stats_Static stats (NOLOCK)
    ON stats.customer_id = c.customer_id
LEFT JOIN dbo.DA_Rep rep1 (NOLOCK)
    ON rep1.customer_id = c.customer_id
WHERE ISNULL(ytd.ytd_sales,0)+ISNULL(ytd.lytd_sales,0)>0
AND (@repId='ALL' OR rep1.salesrep_id=@repId)
ORDER BY ISNULL(ytd.ytd_sales,0) DESC
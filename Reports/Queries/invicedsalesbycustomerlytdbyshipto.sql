SELECT c.customer_id
 , c.customer_name
 , s.ship_to_id
 , a.name ship_to_name
 , (rep1.rep + ' - ' + rep1.salesrep_id) as Salesrep
 , ISNULL(ytd.lytd_sales, 0) as lytd
 , ISNULL(ytd.ytd_sales, 0) as ytd
 , ISNULL(ytd.ytd_sales, 0) - ISNULL(ytd.lytd_sales, 0) Change
 , ISNULL((ISNULL(ytd.ytd_sales, 0) - ISNULL(ytd.lytd_sales, 0)) / NULLIF(ytd.lytd_sales, 0), 0) * 100 as [percent]
 , ISNULL(ytd.ly_sales, 0) as ly
 , stats.FirstSl as first_si
 , stats.LastSl as last_si
 , stats.LastSlDays as last_si_day
 FROM p21_view_customer (NOLOCK) c
 JOIN dbo.p21_view_ship_to s ON s.customer_id = c.customer_id
 JOIN p21_view_address a ON a.id = s.ship_to_id
 JOIN dbo.tbl_ytd_shipto_static (NOLOCK) ytd ON ytd.ship_to_id = s.ship_to_id
 LEFT OUTER JOIN dbo.[DA_SHIPTO_STATS_STATIC] stats ON stats.ship_to_id = s.ship_to_id
 LEFT OUTER JOIN dbo.DA_Rep (NOLOCK) rep1 ON rep1.customer_id = c.customer_id
 WHERE ( @repId = 'ALL' OR rep1.salesrep_id = @repId)
 AND (@custclass = 'ALL' 
 OR (@custclass = 'B2B' AND (c.class_1id != 'ADS' OR c.class_1id IS NULL))
 OR (@custclass = 'ADS' AND c.class_1id = 'ADS'))
 And ISNULL(ytd.ytd_sales, 0) + ISNULL(ytd.ly_sales, 0) > 0
 ORDER BY c.customer_id, (ISNULL(ytd.ytd_sales, 0) - ISNULL(ytd.lytd_sales, 0));
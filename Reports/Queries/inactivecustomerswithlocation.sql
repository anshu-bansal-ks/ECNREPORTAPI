SELECT custstat.customer_id
, c.customer_name
, custstat.LastSl as Last_SI
, custstat.LastSlDays as Last_Sl_Days
, ISNULL(ytd.ytd_sales,0) YTD_SALES
, isnull(ytd.ly_sales, 0) LY_SALES
, rep
, a.phys_city as city
, a.phys_state as state
FROM dbo.DA_Cust_Stats_Static custstat
JOIN dbo.da_ytd_static ytd ON ytd.customer_id = custstat.customer_id
JOIN dbo.DA_Rep ON DA_Rep.customer_id = custstat.customer_id
JOIN p21_view_customer c ON c.customer_id = custstat.customer_id
JOIN p21_view_address a ON a.id = c.customer_id
WHERE ( @repId = 'ALL' OR DA_Rep.salesrep_id = @repId )
And custstat.LastSlDays > @daysold 
And custstat.LastSlDays < @maxdaysold
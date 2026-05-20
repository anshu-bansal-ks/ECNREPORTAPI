SELECT DA_AgingOld.customer_id
, DA_AgingOld.customer_name
, rep
, LastSlDays as days_inactive
, DA_AgingOld.B1 as under_120
, DA_AgingOld.B2 as [120-150]
, DA_AgingOld.B3 as [151-180]
, DA_AgingOld.B4 as over_180
, DA_AgingOld.Tot as total
FROM DA_AgingOld (NOLOCK)
JOIN DA_CUST_STATS (NOLOCK) ON DA_CUST_STATS.customer_id = DA_AgingOld.customer_id
JOIN DA_Rep (NOLOCK) ON DA_Rep.customer_id = DA_AgingOld.customer_id
WHERE LastSlDays > @daysold 
And LastSlDays < @maxdaysold
AND Tot != 0
ORDER BY DA_AgingOld.customer_name
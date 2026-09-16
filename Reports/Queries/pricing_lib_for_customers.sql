SELECT c.customer_id 
, c.customer_name 
, rep 
, pl.price_library_id 
, pl.description 
, DA_Cust_Stats_Static.LastSl as Last_SI
FROM dbo.price_library_x_cust_x_cmpy(NOLOCK) AS plxcxc 
JOIN dbo.price_library(NOLOCK) AS pl ON pl.price_library_uid = plxcxc.price_library_uid 
JOIN customer(NOLOCK) c ON c.customer_id = plxcxc.customer_id 
AND c.company_id = plxcxc.company_id 
JOIN DA_Rep ON DA_Rep.customer_id = c.customer_id 
LEFT OUTER JOIN dbo.DA_Cust_Stats_Static(NOLOCK) ON DA_Cust_Stats_Static.customer_id = c.customer_id 
WHERE (@releasedate IS NULL OR DA_Cust_Stats_Static.LastSl >= CAST(@releasedate AS DATE) OR DA_Cust_Stats_Static.LastSl IS NULL) 
AND (@custId = '' OR @custId = 'ALL' OR CAST(c.customer_id AS VARCHAR(50)) = @custId)
AND (@prclibId = '' OR @prclibId = 'ALL' OR pl.price_library_id LIKE @prclibId)
AND plxcxc.row_status_flag NOT IN (700, 705) 
AND pl.row_status_flag NOT IN (700, 705)
AND c.delete_flag = 'N' 
ORDER BY pl.description, c.customer_name
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
SELECT lib.customer_id
,c.customer_name
,da_rep.rep
,pl.description
,CASE WHEN lib.row_status_flag = 704 THEN 'ACTIVE'
WHEN lib.row_status_flag = 700 THEN 'DELETED'
WHEN lib.row_status_flag = 705 THEN 'INACTIVE'
ELSE CAST (lib.row_status_flag AS VARCHAR)
END [status]
,lib.date_last_modified as last_date_modified
,lib.last_maintained_by
,c.date_created as customer_build_date
FROM dbo.price_library_x_cust_x_cmpy (NOLOCK) lib
JOIN dbo.price_library (NOLOCK) pl ON lib.price_library_uid = pl.price_library_uid
JOIN customer (NOLOCK) c ON c.customer_id = lib.customer_id
JOIN dbo.DA_Rep (NOLOCK) ON lib.customer_id = dbo.DA_Rep.customer_id
WHERE lib.date_last_modified BETWEEN {dateRange} 
ORDER BY customer_name
,customer_id
,status
,description 
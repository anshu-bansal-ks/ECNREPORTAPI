SELECT column_changed
, key2_value customer_id
, customer_name
, p21_view_customer.date_created as customer_date
, CAST(TRY_CAST(old_value AS DECIMAL(18,4)) AS INT) AS old_limit
, CAST(TRY_CAST(new_value AS DECIMAL(18,4)) AS INT) AS new_limit
, audit_trail.date_created
, audit_trail.created_by
, column_description as [description]
FROM dbo.audit_trail (NOLOCK) JOIN dbo.p21_view_customer ON key2_value = customer_id
WHERE source_area_cd IN ( 1356, 1357 ) 
AND audit_trail.date_created BETWEEN {dateRange}
and column_description = 'CREDIT LIMIT'
ORDER BY audit_trail.date_created DESC
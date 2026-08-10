SELECT trail.date_created
, trail.created_by
, key2_value as customer_id
, c.customer_name
, ISNULL(Total_Due, 0) as Total_Due
, trail.old_value
, trail.new_value
FROM dbo.p21_view_audit_trail_customer_1307 trail
JOIN p21_view_customer c ON c.customer_id = trail.key2_value
LEFT OUTER JOIN dbo.v_aging ON v_aging.customer_id = c.customer_id
WHERE trail.date_created BETWEEN {dateRange} 
AND trail.column_changed LIKE '%Salesrep%'
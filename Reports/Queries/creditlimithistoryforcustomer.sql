SELECT c.customer_id
, c.customer_name
, p21_view_audit_trail_customer_1356.old_value
, p21_view_audit_trail_customer_1356.new_value
, p21_view_audit_trail_customer_1356.date_created
, p21_view_audit_trail_customer_1356.created_by
FROM p21_view_audit_trail_customer_1356
JOIN p21_view_customer c ON c.customer_id = p21_view_audit_trail_customer_1356.key2_value
WHERE p21_view_audit_trail_customer_1356.key2_value = @custId
AND column_description = 'Credit Limit' 
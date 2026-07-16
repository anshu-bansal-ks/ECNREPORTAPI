SELECT audit_trail.key1_value as order_no,
customer.customer_id,
customer.customer_name,
audit_trail.date_created as Date_Released,
audit_trail.created_by as Released_By,
terms_desc
FROM p21_view_audit_trail_oe_hdr_1319 audit_trail 
join oe_hdr(nolock) on key1_value = order_no 
join customer (nolock) on customer.customer_id = oe_hdr.customer_id 
join terms(nolock) on customer.terms_id = terms.terms_id
where audit_trail.date_created BETWEEN {dateRange}
and audit_trail.table_changed = 'oe_hdr'
and audit_trail.old_value = 'Hold'
and audit_trail.new_value = 'Approved'
ORDER BY audit_trail.date_created ASC
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
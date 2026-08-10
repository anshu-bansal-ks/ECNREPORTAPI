SELECT audit_trail.key1_value as Order_No
, oe_hdr.order_date
, ih.invoice_no
, ISNULL(ih.total_amount, 0) AS total_amount
, customer.customer_id
, customer.customer_name
, rep
, ytd.ly_sales
, ytd.ytd_sales
, t.terms_desc
, ISNULL (customer.credit_status, '') as credit_status
, a.[Current]
, a.[31_60] as [31_to_60]
, a.[61_90] as[61_to_90]
, a.Over90 as over_90
, a.Total_Due
, credit_limit
, audit_trail.date_created as Date_Released
, audit_trail.created_by as Released_By
FROM p21_view_audit_trail_oe_hdr_1319 audit_trail(NOLOCK)
JOIN oe_hdr(NOLOCK)ON key1_value=order_no
JOIN customer(NOLOCK)ON customer.customer_id=oe_hdr.customer_id
JOIN dbo.V_YTD_3YR ytd ON ytd.customer_id=customer.customer_id
JOIN dbo.DA_Rep rep ON rep.customer_id=customer.customer_id
JOIN v_aging a ON a.customer_id=customer.customer_id
JOIN p21_view_terms t ON t.terms_id=customer.terms_id
LEFT OUTER JOIN p21_view_invoice_hdr ih ON ih.order_no=oe_hdr.order_no
WHERE audit_trail.date_created BETWEEN {dateRange} 
AND audit_trail.column_changed='validation_status'
AND audit_trail.table_changed='oe_hdr'
AND audit_trail.old_value='Hold'
AND audit_trail.new_value='Approved'
ORDER BY a.customer_name, audit_trail.date_created ASC
SELECT c.customer_id
, c.customer_name
, c.credit_limit
, c.credit_status
, t.terms_desc
, r.rep
, s.LastSl as last_si
, c.invoice_batch_uid as invoice_uid
, ib.invoice_batch_desc as invoice_method
, c.statement_batch_uid as statement_uid
, sb.invoice_batch_desc as statement_method
, a.email_address
, c.created_by
FROM p21_view_customer (NOLOCK) c
JOIN dbo.p21_view_address (NOLOCK) a ON a.id = c.customer_id
JOIN dbo.DA_Cust_Stats_Static (NOLOCK) s ON s.customer_id = c.customer_id
JOIN dbo.p21_view_invoice_batch (NOLOCK) ib ON ib.invoice_batch_uid = c.invoice_batch_uid
JOIN dbo.p21_view_invoice_batch (NOLOCK) sb ON sb.invoice_batch_uid = c.statement_batch_uid
JOIN DA_Rep (NOLOCK) r ON r.customer_id = c.customer_id
JOIN dbo.p21_view_terms (NOLOCK) t ON t.terms_id = c.terms_id
WHERE ( c.statement_batch_uid = 2 OR c.invoice_batch_uid = 2 )
AND( a.email_address IS NULL OR a.email_address = '')
AND s.LastSlDays < 365
AND c.delete_flag = 'N'
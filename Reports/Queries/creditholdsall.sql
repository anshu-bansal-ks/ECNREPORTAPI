SELECT oe_hdr.customer_id,
customer.customer_name,
terms.terms_desc,
a.terms_desc AS orderHeaderTerms,
credit_limit,
credit_limit_used,
customer.credit_status,
carrier.name AS carrier,
       oe_hdr.order_no,
oe_hdr.order_date,
SUM((oe_line.qty_ordered - oe_line.qty_invoiced - oe_line.qty_canceled) / oe_line.pricing_unit_size * oe_line.unit_price) AS order_total,
oe_hdr.validation_status,
oe_hdr.job_name,
oe_hdr.po_no,
(first_name + ' ' + last_name) AS salesrep,
DATEDIFF(minute, oe_hdr.date_created, GETDATE()) AS Time_In_Q
FROM oe_hdr WITH (NOLOCK)
JOIN oe_line WITH (NOLOCK) ON oe_line.order_no = oe_hdr.order_no 
AND oe_line.complete = 'N' 
AND oe_line.parent_oe_line_uid = 0
JOIN customer WITH (NOLOCK) ON customer.customer_id = oe_hdr.customer_id 
AND customer.company_id = oe_hdr.company_id
JOIN contacts WITH (NOLOCK) ON customer.salesrep_id = contacts.id
JOIN terms WITH (NOLOCK) ON customer.terms_id = terms.terms_id
JOIN terms a WITH (NOLOCK) ON oe_hdr.terms = a.terms_id
LEFT OUTER JOIN p21_view_address carrier ON carrier.id = oe_hdr.carrier_id
WHERE (oe_hdr.validation_status = 'Hold' OR oe_hdr.validation_status = 'COD')
AND oe_hdr.completed = 'N'
AND oe_hdr.approved = 'Y'
AND oe_hdr.delete_flag = 'N'
AND oe_hdr.projected_order = 'N'
GROUP BY oe_hdr.order_no,
oe_hdr.order_date,
oe_hdr.date_created,
oe_hdr.validation_status,
oe_hdr.customer_id,
customer.customer_name,
terms.terms_desc,
credit_limit,
credit_limit_used,
customer.credit_status,
carrier.name,
oe_hdr.job_name,
oe_hdr.po_no,
first_name,
last_name,
a.terms_desc
ORDER BY customer.customer_name,
oe_hdr.order_no
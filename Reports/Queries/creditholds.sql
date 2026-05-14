SELECT oe_hdr.customer_id, 
(customer.customer_name + '/'+ (first_name + ' ' + last_name)) as [name/rep],
oe_hdr.order_no,
oe_hdr.order_date, 
terms.terms_desc as [Terms/Status], 
sum((oe_line.qty_ordered - oe_line.qty_invoiced - oe_line.qty_canceled)/oe_line.pricing_unit_size * oe_line.unit_price) as order_total,
CAST(datediff(minute,oe_hdr.order_date, getdate()) as INT) as Time_In_Q,
credit_limit,
da_aging.b1 as [current],
da_aging.b2 as [31-60],
da_aging.b3 as [61-90],
da_aging.b4 as [Over_90],
da_aging.tot as total
FROM oe_hdr with (nolock) 
JOIN oe_line with (nolock) ON (oe_line.order_no = oe_hdr.order_no) AND
(oe_line.complete = 'N') AND
(oe_line.parent_oe_line_uid = 0)
JOIN customer with (nolock) ON (customer.customer_id = oe_hdr.customer_id) 
AND (customer.company_id = oe_hdr.company_id)
JOIN contacts with (nolock) on customer.salesrep_id = contacts.id
JOIN terms with (nolock) on customer.terms_id = terms.terms_id
JOIN terms a with (nolock) on oe_Hdr.terms = a.terms_id
LEFT OUTER JOIN da_aging on da_aging.customer_id = oe_hdr.customer_id
WHERE (oe_hdr.validation_status = 'Hold' OR oe_hdr.validation_status = 'COD') AND
(oe_hdr.completed = 'N') AND  
(oe_hdr.approved = 'Y') AND   
(oe_hdr.delete_flag = 'N') AND
(oe_hdr.projected_order = 'N')
Group BY oe_hdr.order_no, oe_hdr.order_date, oe_hdr.date_created, oe_hdr.date_last_modified,
oe_hdr.last_maintained_by, oe_hdr.validation_status, oe_hdr.oe_hdr_uid, oe_hdr.customer_id,
oe_hdr.approved, oe_hdr.taker, oe_hdr.source_location_id, oe_hdr.location_id, oe_hdr.company_id,
oe_hdr.terms, customer.customer_name, oe_hdr.job_name, oe_hdr.job_price_hdr_uid,
oe_hdr.po_no, first_name + ' ' + last_name, terms.terms_desc,	
credit_limit, credit_limit_used, customer.credit_status, a.terms_desc,
da_aging.b1,da_aging.b2,da_aging.b3,da_aging.b4, da_aging.tot
ORDER BY customer.customer_name, oe_hdr.order_no
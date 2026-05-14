select customer.customer_id
, customer.customer_name
, terms_desc as credit_terms
, credit_status_desc as credit_status
from customer (nolock) 
JOIN terms(nolock) on terms.terms_id = customer.terms_id 
JOIN credit_status (nolock) on credit_status.credit_status_id = customer.credit_status
where terms_desc LIKE '%HOLD%' 
AND customer.credit_status not in ('HOLD', 'BOUNCEH','%LEGAL%', '5', '6','7','8') 
AND customer.delete_flag = 'N'
order by customer_name
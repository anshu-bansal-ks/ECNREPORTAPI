select customer.customer_id
, customer.customer_name
, terms_desc as credit_terms
, credit_status_desc as credit_status
from customer (nolock) 
JOIN terms(nolock) on terms.terms_id = customer.terms_id 
JOIN credit_status (nolock) on credit_status.credit_status_id = customer.credit_status
where credit_status_desc like '%BOUNCE%' 
AND customer.delete_flag = 'N'
Order by customer_name
select customer.customer_id
, customer.customer_name
, credit_status
, terms_desc
, LastSl as last_sale
from customer(nolock) 
JOIN da_cust_stats_static(nolock) on customer.customer_id = da_cust_stats_static.customer_id 
JOIN terms (nolock) on customer.terms_id = terms.terms_id
where credit_limit = 1 
AND lastsldays < 90
order by customer.customer_name
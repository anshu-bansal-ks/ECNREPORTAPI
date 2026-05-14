select customer.customer_id
, customer.customer_name
, credit_status
, [current]
, [31_60] as [31-60]
, [61_90] as [61-90]
, Over90 
, total_due
from customer(nolock) 
JOIN da_buckets(nolock) on customer.customer_id = da_buckets.customer_id
where credit_limit = 0 
AND total_due > 0
order by customer.customer_name
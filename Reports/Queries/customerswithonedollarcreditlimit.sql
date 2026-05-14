select customer.customer_id
, customer.customer_name
, credit_status
, terms_desc
, [current]
, [31_60] as [31-60]
, [61_90] as [61-90]
, Over90 as [Over_90]
, total_due
from customer(nolock) 
JOIN da_buckets(nolock) on customer.customer_id = da_buckets.customer_id
JOIN p21_view_terms ON p21_view_terms.terms_id = customer.terms_id
where credit_limit = 1 
AND (ISNULL(@stockable, 'false') = 'false' OR total_due > 0)
order by customer.customer_name
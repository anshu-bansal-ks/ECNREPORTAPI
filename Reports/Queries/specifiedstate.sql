SELECT customer.customer_id
, customer.customer_name
, (first_name + ' ' + last_name) as Rep
, address.phys_city as city
, address.phys_state as 'state'
, [current]
, [31_60] as [31-60]
, [61_90] as [61-90]
, [Over90] as [Over_90]
, Total_due as total
from customer(nolock) 
JOIN contacts(nolock) on contacts.id = customer.salesrep_id 
JOIN address(nolock) on customer.customer_id = address.id 
JOIN da_buckets on da_buckets.customer_id = customer.customer_id
where address.phys_state = @state
AND customer.delete_flag = 'N'
AND total_due > 0
order by customer.customer_name
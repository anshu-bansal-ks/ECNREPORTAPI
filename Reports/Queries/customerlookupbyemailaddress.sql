select customer.customer_id
,customer.customer_name
,address.phys_city
,address.phys_state
,(contacts.first_name + ' ' + contacts.last_name) as rep
,address.email_address
from customer with (nolock),
address with (nolock),
contacts with (nolock)
where customer.customer_id=address.id
and customer.salesrep_id=contacts.id
and address.email_address like '%' + @emailaddress + '%'
and customer.delete_flag='N'
order by customer.customer_name
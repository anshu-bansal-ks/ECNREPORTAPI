select customer.customer_id
, address.name
, oe_hdr.order_no
, oe_hdr.order_date
, oe_hdr.job_name
from oe_hdr with (nolock)
, address with (nolock)
, customer with (nolock)
, contacts with (nolock)
where oe_hdr.customer_id=customer.customer_id
and oe_hdr.customer_id=address.id
and customer.salesrep_id=contacts.id
and customer.salesrep_id=@repId
and oe_hdr.delete_flag='N'
and oe_hdr.completed !='Y'
and oe_hdr.rma_flag='N'
and oe_hdr.approved='N'
and oe_hdr.cancel_flag='N'
and oe_hdr.projected_order='N'
order by address.name
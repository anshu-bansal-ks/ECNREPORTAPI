select distinct ship_to.ship_to_id,
invoice_hdr.customer_id,
address2.name,
ship_to.default_carrier_id as carrier_id,
address.name as 'FREIGHT' ,
sum(invoice_hdr.total_amount)as 'total_sales',
contacts.first_name + ' ' + left(contacts.last_name,1)+ '.' as 'Rep',
(select LTRIM(price_library_id)  + char(10) 
from price_library_x_cust_x_cmpy with (nolock)
join price_library with (nolock) on price_library.price_library_uid = price_library_x_cust_x_cmpy.price_library_uid
where price_library_x_cust_x_cmpy.row_status_flag = 704 
and price_library_x_cust_x_cmpy.customer_id = invoice_hdr.customer_id  for xml path('')) as discount,
paytype.payment_type_desc,
isnull(da_buckets.over90,0) as over_90
from ship_to with (nolock) 
join address with (nolock) on ship_to.default_carrier_id = address.id 
join address address2 with (nolock) on ship_to.ship_to_id = address2.id 
join invoice_hdr with (nolock) on invoice_hdr.ship_to_id = ship_to.ship_to_id 
join customer with (nolock) on customer.customer_id = invoice_hdr.customer_id 
left outer join da_buckets with (nolock) on da_buckets.customer_id = customer.customer_id 
join contacts with (nolock) on contacts.id = customer.salesrep_id 
left outer join (select price_library_x_cust_x_cmpy.customer_id,
price_library.price_library_id
from price_library_x_cust_x_cmpy with (nolock) join price_library with (nolock) on price_library.price_library_uid = price_library_x_cust_x_cmpy.price_library_uid
where price_library_x_cust_x_cmpy.row_status_flag = 704 
) as DISCOUNT on invoice_hdr.customer_id = discount.customer_id 
left outer join (select customer.customer_id,
customer.customer_name,
payment_types.payment_type_desc
from ar_receipts with (nolock) 
join customer with (nolock) on customer.customer_id = ar_receipts.remitter_id 
join ar_payment_details with (nolock) on ar_payment_details.payment_number = ar_receipts.payment_number 
join payment_types with (nolock) on payment_types.payment_type_id = ar_payment_details.payment_type_id
where ar_receipts.payment_number in (
select max(ar_payment_details.payment_number) as 'payment_number'
from ar_payment_details with (nolock) 
join ar_receipts with (nolock) on ar_payment_details.payment_number = ar_receipts.payment_number 
join ar_receipts_detail with (nolock) on ar_receipts.receipt_number = ar_receipts_detail.receipt_number 
join payment_types with (nolock) on payment_types.payment_type_id = ar_payment_details.payment_type_id 
join customer with (nolock) on customer.customer_id = ar_receipts_detail.customer_id
where payment_types.payment_type_desc <> 'MISC'
group by customer.customer_id)) as PAYTYPE on invoice_hdr.customer_id = paytype.customer_id
where (address.name like '%CHARGE%' or address.name like '%FLAT%'
or address.name like '%FREE%')
AND invoice_hdr.invoice_date BETWEEN {dateRange}
and invoice_hdr.customer_id <> 110841
AND customer.salesrep_id <> 5325
group by ship_to.ship_to_id,
invoice_hdr.customer_id,
address2.name,
ship_to.default_carrier_id,
address.name,
contacts.first_name + ' ' + left(contacts.last_name,1)+ '.',
discount.price_library_id,
paytype.payment_type_desc,
da_buckets.over90
order by address2.name
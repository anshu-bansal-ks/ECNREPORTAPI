 select customer.customer_id
 , customer.customer_name
 , terms_desc
 , B1 as [current]
 , B2 as [31-60]
 , B3 as [61-90]
 , B4 as [Over_90]
 , Tot as total
 from customer 
 join da_aging(nolock) on da_aging.customer_id = customer.customer_id 
 join terms on customer.terms_id = terms.terms_id
 where terms_desc = 'PENDING' 
 Order By customer.customer_name
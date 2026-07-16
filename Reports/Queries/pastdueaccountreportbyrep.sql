select customer.customer_id
, address.name
, (contacts.first_name+ '  ' +last_name) as rep
, max(datediff(dd,invoice_hdr.invoice_date,getdate())) as Oldest_Invoice
, sum(invoice_hdr.total_amount - invoice_hdr.amount_paid - invoice_hdr.terms_taken - invoice_hdr.allowed + invoice_hdr.memo_amount + invoice_hdr.bad_debt_amount) as Total_Balance
, DA_buckets.[Current] as Total_Current
, DA_buckets.[31_60] as [Total_31-60]
, DA_buckets.[61_90] as [Total_61-90]
, DA_buckets.Over90 as [Total_Over_90]
, da_lastnote.date_created as Last_Ar_Note
FROM invoice_hdr 
join customer(nolock) on invoice_hdr.customer_id = customer.customer_id 
join address(nolock) on customer.customer_id=address.id 
join contacts(nolock) on contacts.id = customer.salesrep_id 
join DA_buckets(nolock) on DA_buckets.customer_id = customer.customer_id 
left outer join da_lastnote(nolock) on da_lastnote.customer_id = customer.customer_id
WHERE invoice_hdr.paid_in_full_flag='N'
GROUP BY customer.customer_id
, address.name
, contacts.first_name
, contacts.last_name
, DA_buckets.[Current]
, DA_buckets.[31_60]
, DA_buckets.[61_90]
, DA_buckets.Over90
, da_lastnote.date_created
HAVING max(datediff(dd,invoice_hdr.invoice_date,getdate())) > @daysold
order by contacts.last_name
, contacts.first_name
, address.name
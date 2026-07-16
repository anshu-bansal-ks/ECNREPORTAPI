SELECT customer_call.date_created
,customer_call.notes
FROM customer (NOLOCK)
,customer_call
WHERE customer_call.customer_id = customer.customer_id 
AND customer.customer_id=@custId
ORDER BY customer_call.date_created DESC
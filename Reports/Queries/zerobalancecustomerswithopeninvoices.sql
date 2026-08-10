Select customer.customer_id
, customer.customer_name
FROM customer 
JOIN p21_invoice_amt_remaining_view on p21_invoice_amt_remaining_view.customer_id = customer.customer_id
WHERE p21_invoice_amt_remaining_view.paid_in_full_flag = 'N'
GROUP BY customer.customer_name
, customer.customer_id
HAVING sum(p21_invoice_amt_remaining_view.amt_remaining_frominv)=0
ORDER BY customer.customer_name
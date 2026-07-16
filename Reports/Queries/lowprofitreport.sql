select invoice_hdr.customer_id,
ship2_name,
invoice_date,
invoice_no,
order_no,
(first_name + ' ' + last_name) as rep,
(total_amount - freight) as sales_amount,
shipping_cost,
(total_amount - freight - shipping_cost) as gross_profit,
(case when total_amount - freight =0 then 0
else round((total_amount - freight - shipping_cost) / (total_amount - freight) * 100, 4) end) as [profit_percent]
from invoice_hdr(nolock) 
join contacts(nolock) on salesrep_id = id
JOIN customer (NOLOCK) cust ON cust.customer_id = invoice_hdr.customer_id
where invoice_date BETWEEN {dateRange}
AND (@custclass = 'ALL' 
 OR (@custclass = 'B2B' AND (cust.class_1id != 'ADS' OR cust.class_1id IS NULL))
 OR (@custclass = 'ADS' AND cust.class_1id = 'ADS'))
AND total_amount > 1 
and total_amount - freight <> 0 
and ship2_name not like '%EFFEX%'
group by invoice_hdr.customer_id,
ship2_name,
invoice_date,
invoice_no,
order_no,
first_name + ' ' + last_name,
total_amount - freight,
shipping_cost,
total_amount - freight - shipping_cost,
(total_amount - freight - shipping_cost) / (total_amount - freight)*100 ,
total_amount,
freight
having (case when total_amount - freight =0
then 0 
else round((total_amount - freight - shipping_cost) / (total_amount - freight), 4) * 100 end)<= @profit
order by first_name + ' ' + last_name 




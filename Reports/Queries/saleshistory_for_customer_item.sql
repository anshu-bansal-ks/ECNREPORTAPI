select il.item_id
, im.item_desc
, CAST(sum(qty_shipped) AS INT) as qty
, sum(il.extended_price) as SALES
from invoice_hdr ih (nolock)
join invoice_line (nolock) il on ih.invoice_no = il.invoice_no
join inv_mast (nolock) im on im.inv_mast_uid = il.inv_mast_uid
join customer (nolock) cust on cust.customer_id = ih.customer_id
where ih.customer_id = @custId
and ih.invoice_date BETWEEN {dateRange}
group by cust.customer_id
, cust.customer_name
, il.item_id, im.item_desc
order by item_id
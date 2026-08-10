select da_sales_notes.date_added as 'date',
da_sales_notes.comment as notes
from da_sales_notes with (nolock) 
JOIN address with (nolock) on da_sales_notes.customer_id = address.id
where bln_valid = 1 
and da_sales_notes.customer_id =@custId
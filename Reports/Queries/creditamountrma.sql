select oe_hdr.order_no as rma_no
 ,oe_hdr.order_date as RMA_date
 ,invoice_hdr.invoice_no as credit_no
 ,invoice_hdr.invoice_date as credit_date
 ,COALESCE(invoice_hdr.total_amount,0) as [amount]
 from oe_hdr with (nolock) 
 left outer join invoice_hdr with (nolock) on oe_hdr.order_no = invoice_hdr.order_no 
 join customer with (nolock) on oe_hdr.customer_id = customer.customer_id
 where rma_flag = 'Y' 
 and oe_hdr.customer_id = @custId
 order by oe_hdr.order_date desc
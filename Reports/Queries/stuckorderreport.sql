SELECT oh.order_no
 , oh.customer_id
 , c.customer_name
 , oh.order_date
 , oh.date_last_modified 
 , oh.projected_order quote
 , oh.po_no
 , oh.created_by
 , oh.last_maintained_by
 FROM p21_view_oe_hdr oh
 JOIN p21_view_customer c ON c.customer_id = oh.customer_id
 WHERE (oh.completed = 'T')
 AND (oh.rma_flag = 'N')
 AND DATEDIFF(mi, oh.date_last_modified, GETDATE()) > 240 
 ORDER BY oh.order_date DESC 
 , c.date_created
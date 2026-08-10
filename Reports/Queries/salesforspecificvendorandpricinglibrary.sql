 with cte as(
SELECT ih.customer_id
, ih.bill2_name
,ih.order_no
,l.supplier_id
,l.line_no
,l.extended_price
FROM p21_view_invoice_hdr  ih
JOIN p21_view_invoice_line l ON l.invoice_no=ih.invoice_no 
AND l.supplier_id=@supplierId
WHERE ih.date_created BETWEEN {dateRange})
select t.customer_id
, t.bill2_name as bill_to_name
, t.supplier_id
, s.supplier_name
, pp.description as price_page
, SUM (t.extended_price) as sales
from cte t
JOIN p21_view_oe_hdr oh ON oh.order_no=t.order_no
JOIN p21_view_oe_line ol ON ol.oe_hdr_uid=oh.oe_hdr_uid 
AND ol.price_page_uid=@pageprice
AND ol.line_no=t.line_no
JOIN p21_view_price_page pp ON pp.price_page_uid=ol.price_page_uid
JOIN p21_view_supplier s ON s.supplier_id=t.supplier_id
GROUP BY t.customer_id
, t.bill2_name
, ol.price_page_uid
, pp.description
, t.supplier_id
, s.supplier_name
ORDER BY s.supplier_name, pp.description
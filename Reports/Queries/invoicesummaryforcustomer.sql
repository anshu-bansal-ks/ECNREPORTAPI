SELECT c.customer_id
, c.customer_name
, ih.po_no
, ih.invoice_no
, ih.invoice_date
, ih.order_no
, ih.total_amount-freight as Sales
, ih.freight
, ih.total_amount
FROM p21_view_invoice_hdr ih
JOIN p21_view_customer c ON c.customer_id=ih.customer_id
WHERE ih.customer_id=@custId
And invoice_date BETWEEN {dateRange} 
ORDER by ih.invoice_no
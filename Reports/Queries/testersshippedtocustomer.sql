SELECT ihs.salesrep_id
, cust.customer_id
, cust.customer_name
, (c.first_name+' '+c.last_name) as Rep
, c.last_name
, c.first_name
, il.item_id
, im.item_desc
, SUM (qty_shipped) as qty
FROM invoice_hdr ih(NOLOCK)
JOIN invoice_line(NOLOCK) il ON ih.invoice_no=il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs(NOLOCK)ON ih.invoice_no=ihs.invoice_number
JOIN contacts c(NOLOCK)ON c.id=ihs.salesrep_id
JOIN inv_mast(NOLOCK) im ON im.inv_mast_uid=il.inv_mast_uid
JOIN customer(NOLOCK) cust ON cust.customer_id=ih.customer_id
LEFT OUTER JOIN inv_mast_ud ud ON ud.inv_mast_uid=im.inv_mast_uid
WHERE 1=1
AND ih.customer_id=@custId
AND ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep='Y'
AND ud.tester='Y'
GROUP BY ihs.salesrep_id
, cust.customer_id
, cust.customer_name
, c.first_name+' '+c.last_name
, c.last_name
, c.first_name
, il.item_id
, im.item_desc
ORDER BY item_id
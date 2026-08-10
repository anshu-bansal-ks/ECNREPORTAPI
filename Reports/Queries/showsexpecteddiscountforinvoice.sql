SELECT ih.invoice_no
, (c.first_name +' '+ c.last_name) as Rep
, il.supplier_id
, s.supplier_name
, SUM (qty_shipped) as UNITS
, SUM (il.extended_price) as SALES
, SUM (IIF(ss.supplier_id IS NOT NULL, il.extended_price, 0)) as SHOW_SALES
, IIF(ss.discount IS NOT NULL, IIF(discount=0, 0, discount / 100), 0) as discount
, ROUND (SUM (IIF(ss.supplier_id IS NOT NULL, il.extended_price, 0)) * IIF(ss.discount IS NOT NULL, IIF(discount=0, 0, discount / 100), 0), 2) as expected_discount
, SUM (IIF(ss.supplier_id IS NULL, il.extended_price, 0)) as NON_SHOW_SALES
FROM invoice_hdr ih(NOLOCK)
JOIN customer(NOLOCK) cust ON cust.customer_id=ih.customer_id
JOIN invoice_line(NOLOCK) il ON ih.invoice_no=il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs(NOLOCK)ON ih.invoice_no=ihs.invoice_number
JOIN contacts c(NOLOCK)ON c.id=ihs.salesrep_id
JOIN inv_mast(NOLOCK) im ON im.inv_mast_uid=il.inv_mast_uid
JOIN p21_view_oe_hdr oh ON oh.order_no=ih.order_no
JOIN p21_view_supplier s ON s.supplier_id=il.supplier_id
LEFT JOIN {dashboard}.dbo.ShowsSupplier ss ON ss.supplier_id=il.supplier_id AND ShowId=@showId
WHERE ihs.primary_salesrep='Y'
AND ih.invoice_no=@invoicenum
GROUP BY ih.invoice_no
, c.first_name+' '+c.last_name
, c.last_name
, c.first_name
, il.supplier_id
, s.supplier_name
, ss.discount
ORDER BY s.supplier_name
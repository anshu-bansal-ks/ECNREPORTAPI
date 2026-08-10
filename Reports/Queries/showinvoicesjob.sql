SELECT r.salesrep_id as rep_id
, r.rep
, ih.customer_id
, ih.bill2_name
, ih.ship2_name as ship_to_name
, ih.po_no
, oh.job_name
, ih.invoice_no
, ih.invoice_date
, SUM (il.extended_price) as SALES
, FREIGHT.freight
, ih.total_amount
, ISNULL( DISCOUNT.disc_amt,0) disc_amount
FROM p21_view_invoice_hdr ih
JOIN p21_view_invoice_line il ON ih.invoice_no=il.invoice_no
JOIN DA_Rep r ON r.customer_id=ih.customer_id
JOIN p21_view_oe_hdr oh ON oh.order_no=ih.order_no
JOIN ( SELECT invoice_hdr.invoice_no
, freight
FROM invoice_hdr (NOLOCK)
JOIN p21_view_oe_hdr oh ON oh.order_no=invoice_hdr.order_no
WHERE invoice_date BETWEEN {dateRange}
AND(job_name LIKE '%'+@job_name+'%')
) AS FREIGHT ON FREIGHT.invoice_no=ih.invoice_reference_no
LEFT OUTER JOIN ( SELECT invoice_hdr.invoice_no
, il.item_id
, SUM (il.extended_price) disc_amt
FROM invoice_hdr (NOLOCK)
JOIN p21_view_invoice_line il ON il.invoice_no=invoice_hdr.invoice_no
JOIN p21_view_oe_hdr oh ON oh.order_no=invoice_hdr.order_no
WHERE invoice_date BETWEEN {dateRange}
AND(job_name LIKE '%'+@job_name+'%')
AND il.item_id=@itemId
GROUP BY invoice_hdr.invoice_no
, il.item_id
) AS DISCOUNT ON DISCOUNT.invoice_no=ih.invoice_reference_no
WHERE(oh.job_name LIKE '%'+@job_name+'%')
AND ih.invoice_date BETWEEN {dateRange}
GROUP BY r.salesrep_id
, ih.customer_id
, ih.bill2_name
, ih.ship2_name
, ih.po_no
, oh.job_name
, ih.invoice_no
, ih.invoice_date
, r.rep
, FREIGHT.freight
, ih.total_amount
, DISCOUNT.disc_amt
ORDER BY ih.invoice_no
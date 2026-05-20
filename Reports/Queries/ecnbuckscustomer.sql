SELECT ih.customer_id
, c.customer_name
, rep1.rep
, SUM(il.extended_price) as total
FROM ccecn.dbo.p21_view_invoice_hdr ih
JOIN ccecn.dbo.p21_view_customer c ON c.customer_id = ih.customer_id
JOIN ccecn.dbo.p21_view_invoice_line il ON il.invoice_no = ih.invoice_no
JOIN ccecn.dbo.p21_view_supplier s ON s.supplier_id = il.supplier_id
Inner JOIN dbo.DA_Rep (NOLOCK) rep1 ON rep1.customer_id = c.customer_id
Join {dashboard}.dbo.ShowsSupplier (NOLOCK) tbl on tbl.supplier_id = il.supplier_id 
WHERE ih.invoice_date BETWEEN {dateRange} 
AND ih.po_no LIKE @po_no + '%' 
And tbl.ShowId=@ShowId
GROUP BY  ih.customer_id
, c.customer_name
,rep1.rep
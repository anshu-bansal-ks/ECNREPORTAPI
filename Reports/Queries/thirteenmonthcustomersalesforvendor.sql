SELECT 
    DA_Rep.rep, 
    CAST(YEAR(invoice_date) AS VARCHAR(4)) + '_' + RIGHT('0' + CAST(MONTH(invoice_date) AS VARCHAR(2)), 2) yr_mnth,
    ih.customer_id, 
    c.customer_name, 
    il.supplier_id, 
    s.supplier_name, 
    CAST(ISNULL(il.extended_price, 0) AS DECIMAL(18,2)) as total_amount
FROM p21_view_invoice_hdr ih WITH (NOLOCK)
JOIN p21_view_invoice_line il ON il.invoice_no = ih.invoice_no
JOIN DA_Rep ON DA_Rep.customer_id = ih.customer_id
JOIN p21_view_customer c ON c.customer_id = ih.customer_id
JOIN p21_view_supplier s ON s.supplier_id = il.supplier_id
WHERE DATEDIFF(mm, invoice_date, GETDATE()) < 13 
  AND (@repId = 'ALL' OR DA_Rep.salesrep_id = @repId)
  AND (@supplierId = '0' OR il.supplier_id = @supplierId)
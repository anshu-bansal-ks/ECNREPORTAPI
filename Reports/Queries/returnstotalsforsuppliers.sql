SELECT p21_view_oe_hdr.source_location_id as location_id
, p21_view_invoice_line.supplier_id
, p21_view_supplier.supplier_name
, SUM(p21_view_invoice_line.extended_price) AS total_value 
FROM p21_view_invoice_hdr (NOLOCK)
JOIN p21_view_invoice_line (NOLOCK) ON p21_view_invoice_hdr.invoice_no = p21_view_invoice_line.invoice_no
JOIN p21_view_supplier (NOLOCK) ON p21_view_supplier.supplier_id = p21_view_invoice_line.supplier_id
JOIN p21_view_oe_hdr (NOLOCK) ON p21_view_oe_hdr.order_no = p21_view_invoice_hdr.order_no
join {dashboard}.dbo.tbl_loc loc on loc.location_id = p21_view_oe_hdr.source_location_id 
WHERE p21_view_invoice_hdr.invoice_date BETWEEN {dateRange}
AND ( @locationId IS NULL OR CAST(@locationId AS VARCHAR) = 'ALL' 
OR CAST(@locationId AS VARCHAR) = '0' 
OR CAST(p21_view_oe_hdr.source_location_id AS VARCHAR) = CAST(@locationId AS VARCHAR))
and loc.loc_type='RETURNS' 
and loc.Company=@compId 
AND p21_view_oe_hdr.rma_flag = 'y'
GROUP BY p21_view_invoice_line.supplier_id
, p21_view_supplier.supplier_name
, p21_view_oe_hdr.source_location_id
ORDER BY p21_view_supplier.supplier_name
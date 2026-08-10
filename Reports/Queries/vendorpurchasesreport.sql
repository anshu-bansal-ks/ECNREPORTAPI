SELECT vendor.vendor_id
,vendor.vendor_name
,sum(p21_view_apinv_hdr.invoice_amount) as invoice_amount
FROM p21_view_apinv_hdr WITH (NOLOCK)
JOIN vendor ON vendor.vendor_id = p21_view_apinv_hdr.vendor_id
WHERE 1 = 1
AND p21_view_apinv_hdr.invoice_date BETWEEN {dateRange}
AND po_no IS NOT NULL
AND voucher_type = 'V'
GROUP BY vendor.vendor_id
,vendor.vendor_name
ORDER BY vendor.vendor_name
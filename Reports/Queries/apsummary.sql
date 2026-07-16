-- Isme sirf pure SQL query rahegi
SELECT vendor.vendor_id 
       ,vendor.vendor_name 
       ,sum(p21_view_apinv_hdr.open_amount) Balance
FROM p21_view_apinv_hdr with (nolock)
JOIN vendor (nolock) ON vendor.vendor_id = p21_view_apinv_hdr.vendor_id
WHERE p21_view_apinv_hdr.paid_in_full='N'
GROUP BY vendor.vendor_name, vendor.vendor_id
ORDER BY vendor.vendor_name;
SELECT hdr.buyer_id
, (contacts.first_name + ' ' + contacts.last_name) as buyer_name
, hdr.supplier_id
, p21_view_supplier.supplier_name
, hdr.date_created
, hdr.last_maintained_by
, hdr.return_number AS return_no
, hdr.rma_number AS rma_no
, return_value.return_value
, hdr.location_id
, code_description as [Status]
FROM dbo.p21_view_inventory_return_hdr hdr
JOIN dbo.p21_view_supplier ON p21_view_supplier.supplier_id=hdr.supplier_id
JOIN dbo.p21_view_codes ON code_no=row_status_flag
AND code_group_no=1038
LEFT OUTER JOIN p21_view_contacts contacts ON contacts.id = hdr.buyer_id
JOIN (SELECT h.return_number
, SUM (l.extended_price) return_value
FROM p21_view_inventory_return_hdr h
JOIN dbo.p21_view_inventory_return_line l ON l.inventory_return_hdr_uid = h.inventory_return_hdr_uid
GROUP BY h.return_number
) return_value ON return_value.return_number=hdr.return_number
WHERE 1=1
AND hdr.row_status_flag<>976 
AND (@stockable = 'true' OR (hdr.row_status_flag <> 975 AND hdr.row_status_flag <> 974))                
ORDER BY hdr.return_number
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
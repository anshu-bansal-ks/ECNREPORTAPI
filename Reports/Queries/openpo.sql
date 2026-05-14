SELECT poh.location_id,l.location_name,poh.order_date,pol.date_due,poh.po_no,
ISNULL(poh.external_po_no, '') AS external_po_no,s.supplier_id,s.supplier_name,
im.item_id,ISNULL(ins.supplier_part_no, '') as supplier_part_no,im.item_desc,
CAST(pol.qty_ordered AS INT) as qty_ordered,
CAST(pol.qty_received AS INT) as qty_received,
CAST((pol.qty_ordered - pol.qty_received) AS INT) AS qty_remaining
FROM p21_view_po_hdr AS poh WITH (NOLOCK)
INNER JOIN p21_view_po_line AS pol WITH (NOLOCK) ON pol.po_no = poh.po_no
INNER JOIN p21_view_supplier AS s WITH (NOLOCK) ON s.supplier_id = poh.supplier_id
INNER JOIN p21_view_inventory_supplier AS ins WITH (NOLOCK)
ON ins.supplier_id = s.supplier_id AND ins.inv_mast_uid = pol.inv_mast_uid
INNER JOIN p21_view_inv_mast AS im WITH (NOLOCK) ON im.inv_mast_uid = pol.inv_mast_uid
INNER JOIN p21_view_location AS l WITH (NOLOCK) ON l.location_id = poh.location_id
WHERE pol.complete = 'N'
AND pol.delete_flag = 'N'
AND pol.cancel_flag = 'N'
AND poh.po_type NOT IN ('Q','X')
AND (
    @supplierId IS NULL 
    OR CAST(@supplierId AS VARCHAR) = 'ALL' 
    OR CAST(@supplierId AS VARCHAR) = '0' 
    OR CAST(s.supplier_id AS VARCHAR) = CAST(@supplierId AS VARCHAR)
)
ORDER BY poh.order_date, poh.po_no, l.location_name
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
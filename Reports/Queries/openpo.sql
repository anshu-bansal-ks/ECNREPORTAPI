SELECT 
    poh.location_id,
    l.location_name,
    poh.order_date,
    pol.date_due,
    poh.po_no,
    ISNULL(poh.external_po_no, '') AS external_po_no,
    s.supplier_id,
    s.supplier_name,
    im.item_id,
    ins.supplier_part_no,
    im.item_desc,
    pol.qty_ordered,
    pol.qty_received,
    (pol.qty_ordered - pol.qty_received) AS qty_remaining
FROM p21_view_po_hdr AS poh WITH (NOLOCK)
INNER JOIN p21_view_po_line AS pol ON pol.po_no = poh.po_no
INNER JOIN p21_view_supplier AS s ON s.supplier_id = poh.supplier_id
INNER JOIN p21_view_contacts AS c ON c.id = poh.requested_by
INNER JOIN p21_view_inventory_supplier AS ins 
    ON ins.supplier_id = s.supplier_id AND ins.inv_mast_uid = pol.inv_mast_uid
INNER JOIN p21_view_inv_mast AS im ON im.inv_mast_uid = pol.inv_mast_uid
INNER JOIN p21_view_location AS l ON l.location_id = poh.location_id
WHERE 1-1
 And pol.complete = 'N'
    AND pol.delete_flag = 'N'
    AND pol.cancel_flag = 'N'
    AND poh.po_type NOT IN ('Q','X')
    -- 🔥 Supplier Filter (MasterReport mapping se @supplierId milega)
    AND (@supplierId IS NULL OR @supplierId = 'ALL' OR s.supplier_id = @supplierId)
    -- 🔥 Date Filter (Agar user lagana chahe toh)
    AND poh.order_date BETWEEN {dateRange})
ORDER BY poh.order_date , poh.po_no, l.location_name
OFFSET (@pageNumber - 1) * @pageSize ROWS
FETCH NEXT @pageSize ROWS ONLY;
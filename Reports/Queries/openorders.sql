SELECT r.salesrep_id
, r.rep
,oe_hdr.address_id AS ship2_id
,oe_hdr.ship2_name
,oe_hdr.order_date
,oe_hdr.order_no
,oe_hdr.po_no
,inv_mast.item_id
,inv_mast.item_desc
,CAST(oe_line.qty_ordered AS INT) as qty_ordered
,CAST(oe_line.qty_on_pick_tickets AS INT) as qty_on_pick_tickets
,CAST(inv_loc.qty_on_hand AS INT) as qty_on_hand
,CAST(inv_loc.qty_allocated AS INT) as qty_allocated
,oe_line.disposition
,CAST(oe_line.qty_invoiced AS INT) as qty_invoiced
FROM oe_hdr WITH (NOLOCK)
JOIN oe_hdr_salesrep(NOLOCK) ON oe_hdr.order_no = oe_hdr_salesrep.order_number
JOIN oe_line(NOLOCK) ON oe_hdr.order_no = oe_line.order_no
JOIN inv_mast WITH (NOLOCK) ON inv_mast.inv_mast_uid = oe_line.inv_mast_uid
JOIN inv_loc WITH (NOLOCK) ON inv_loc.inv_mast_uid = inv_mast.inv_mast_uid
LEFT OUTER JOIN v_qty(NOLOCK) ON v_qty.uid = inv_mast.inv_mast_uid
JOIN address(NOLOCK) ON address.id = oe_hdr.source_location_id
JOIN DA_Rep r ON r.customer_id=oe_hdr.customer_id
WHERE (@repId = 'ALL' OR oe_hdr_salesrep.salesrep_id = @repId )                                      
AND (@status = 'ALL' OR (@status = 'APPROVED' AND oe_hdr.approved = 'Y')
OR (@status = 'UNAPPROVED' AND oe_hdr.approved = 'N')
) 
AND oe_hdr.delete_flag = 'N'
AND oe_line.delete_flag = 'N'
AND oe_line.complete = 'N'
AND inv_loc.location_id = oe_hdr.source_location_id
AND oe_hdr.rma_flag = 'N'
AND oe_hdr.projected_order = 'N'
AND oe_hdr.cancel_flag = 'N'
ORDER BY oe_hdr.order_date DESC
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
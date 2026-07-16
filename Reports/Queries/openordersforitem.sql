SELECT r.rep
, oe_hdr.customer_id
, c.customer_name
, oe_hdr.address_id AS ship2_id
, oe_hdr.ship2_name
, oe_hdr.order_date
, oe_hdr.order_no
, oe_hdr.po_no
, job_name
, inv_mast.item_id
, inv_mast.item_desc
, CAST(oe_line.qty_ordered AS INT) as qty_ordered
, oe_line.unit_price
, oe_hdr.approved
FROM oe_hdr WITH(NOLOCK)
JOIN oe_hdr_salesrep(NOLOCK)ON oe_hdr.order_no=oe_hdr_salesrep.order_number
JOIN oe_line(NOLOCK)ON oe_hdr.order_no=oe_line.order_no
JOIN inv_mast WITH(NOLOCK)ON inv_mast.inv_mast_uid=oe_line.inv_mast_uid
JOIN inv_loc WITH(NOLOCK)ON inv_loc.inv_mast_uid=inv_mast.inv_mast_uid
JOIN p21_view_customer c ON c.customer_id = oe_hdr.customer_id
JOIN address(NOLOCK)ON address.id=oe_hdr.source_location_id
JOIN DA_Rep r ON r.customer_id=oe_hdr.customer_id
WHERE (@repId = 'ALL' OR oe_hdr_salesrep.salesrep_id = @repId )
AND (@status = 'ALL' OR (@status = 'APPROVED' AND oe_hdr.approved = 'Y')
OR (@status = 'UNAPPROVED' AND oe_hdr.approved = 'N')
)
AND oe_hdr.delete_flag='N'
AND oe_line.delete_flag='N'
AND oe_line.complete='N'
AND inv_loc.location_id=oe_hdr.source_location_id
AND oe_hdr.rma_flag='N'
AND oe_hdr.projected_order='N'
AND oe_hdr.cancel_flag='N'
AND inv_mast.item_id = @itemId
ORDER BY oe_hdr.order_date DESC
SELECT oe_hdr_salesrep.salesrep_id
, oe_hdr.address_id AS ship2_id
, oe_hdr.ship2_name
, oe_hdr.order_date
, oe_hdr.order_no
, oe_hdr.po_no
, inv_mast.item_id
, inv_mast.item_desc
,CAST(oe_line.qty_ordered AS INT) as qty_ordered
FROM oe_hdr with (nolock) join 
oe_hdr_salesrep (nolock) on oe_hdr.order_no = oe_hdr_salesrep.order_number join 
oe_line (nolock) on oe_hdr.order_no = oe_line.order_no join 
inv_mast with (nolock) on inv_mast.inv_mast_uid = oe_line.inv_mast_uid join 
inv_loc with (nolock) on inv_loc.inv_mast_uid = inv_mast.inv_mast_uid left outer join  
v_qty (nolock) on v_qty.uid = inv_mast.inv_mast_uid join 
address(nolock) on address.id = oe_hdr.source_location_id 
WHERE (@repId = 'ALL' OR oe_hdr_salesrep.salesrep_id = @repId )
AND (@status = 'ALL' OR (@status = 'APPROVED' AND oe_hdr.approved = 'Y')
OR (@status = 'UNAPPROVED' AND oe_hdr.approved = 'N')
) 
AND oe_hdr.delete_flag = 'N' 
and oe_line.delete_flag = 'N' 
and oe_line.complete = 'N' 
and inv_loc.location_id = oe_hdr.source_location_id  
and oe_hdr.rma_flag = 'N' 
and oe_hdr.projected_order = 'Y' 
and oe_hdr.cancel_flag = 'N' 
ORDER BY ship2_name, oe_hdr.order_date asc
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
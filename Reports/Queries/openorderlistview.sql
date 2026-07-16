SELECT distinct oe_hdr_salesrep.salesrep_id
, oe_hdr.address_id AS ship2_id
, oe_hdr.ship2_name
, oe_hdr.order_date
, oe_hdr.order_no
, oe_hdr.po_no
FROM oe_hdr WITH (nolock) 
INNER JOIN oe_hdr_salesrep WITH (nolock) ON oe_hdr.order_no = oe_hdr_salesrep.order_number 
INNER JOIN oe_line WITH (nolock) ON oe_hdr.order_no = oe_line.order_no 
INNER JOIN inv_mast WITH (nolock) ON inv_mast.inv_mast_uid = oe_line.inv_mast_uid 
INNER JOIN inv_loc WITH (nolock) ON inv_loc.inv_mast_uid = inv_mast.inv_mast_uid 
AND oe_hdr.source_location_id = inv_loc.location_id 
LEFT OUTER JOIN dbo.V_QTY(nolock) AS V_QTY_1 ON V_QTY_1.UID = inv_mast.inv_mast_uid 
INNER JOIN address WITH (nolock) ON address.id = oe_hdr.source_location_id
WHERE (@repId = 'ALL' OR oe_hdr_salesrep.salesrep_id = @repId )                                      
AND (@status = 'ALL' OR (@status = 'APPROVED' AND oe_hdr.approved = 'Y')
OR (@status = 'UNAPPROVED' AND oe_hdr.approved = 'N')
) 
AND (oe_hdr.delete_flag = 'N') 
AND (oe_line.delete_flag = 'N') 
AND (oe_line.complete = 'N') 
AND (oe_hdr.rma_flag = 'N') 
AND (oe_hdr.projected_order = 'N') 
AND (oe_hdr.cancel_flag = 'N')
ORDER BY oe_hdr.ship2_name
, oe_hdr.order_date
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
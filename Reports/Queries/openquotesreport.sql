SELECT oe_hdr_salesrep.salesrep_id
, oe_hdr.order_no as quote_no
, oe_hdr.order_date
, oe_hdr.address_id as ship2_id
, oe_hdr.ship2_name
, oe_hdr.po_no
FROM oe_hdr WITH (NOLOCK)
JOIN oe_hdr_salesrep (NOLOCK) ON oe_hdr.order_no = oe_hdr_salesrep.order_number
WHERE (@repId = 'ALL' OR oe_hdr_salesrep.salesrep_id = @repId )
AND oe_hdr.delete_flag = 'N'
AND oe_hdr.rma_flag = 'N'
AND oe_hdr.projected_order = 'Y'
AND oe_hdr.cancel_flag = 'N'
AND oe_hdr.completed != 'Y'
ORDER BY ship2_name
, oe_hdr.order_date ASC;
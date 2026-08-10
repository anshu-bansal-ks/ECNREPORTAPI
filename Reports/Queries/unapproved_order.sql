SELECT hdr.source_location_id
,hdr.created_by
,hdr.order_no
,order_date
,hdr.customer_id
,hdr.ship2_name
,hdr.po_no
,oeud.rep_note
,rep
,projected_order
FROM oe_hdr (NOLOCK) hdr
JOIN dbo.DA_Rep ON DA_Rep.customer_id = hdr.customer_id
Left Outer join dbo.oe_hdr_ud oeud on oeud.order_no = hdr.order_no
WHERE hdr.rma_flag = 'N'
AND hdr.approved = 'N'
AND hdr.source_location_id=@locationId
AND hdr.completed = 'N'
AND hdr.delete_flag = 'N'
AND hdr.cancel_flag = 'N'
ORDER BY order_date Desc
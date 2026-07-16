SELECT hdr.source_location_id as location_id
, hdr.created_by
, hdr.order_no
, hdr.order_date
, hdr.customer_id
, hdr.ship2_name
, hdr.po_no
, hdr.taker
, rep
, im.item_id
, im.item_desc
, l.extended_price as price
, CAST(l.qty_ordered AS INT) as qty_ordered
, CAST(l.qty_canceled AS INT) as qty_canceled
, CAST(l.qty_invoiced AS INT) as qty_invoiced
, CAST((l.qty_ordered-l.qty_canceled-l.qty_invoiced) AS INT) as qty_open
, hdr.date_last_modified as last_date
FROM p21_view_oe_hdr hdr
JOIN p21_view_oe_line l ON l.oe_hdr_uid=hdr.oe_hdr_uid
JOIN dbo.DA_Rep ON DA_Rep.customer_id=hdr.customer_id
JOIN p21_view_inv_mast im ON im.inv_mast_uid=l.inv_mast_uid
WHERE 1=1
AND hdr.rma_flag='Y'
AND hdr.approved='Y'
AND hdr.completed='N'
AND hdr.delete_flag='N'
AND l.complete='N'
ORDER BY hdr.ship2_name
, hdr.order_no
, im.item_id;
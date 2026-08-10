SELECT oe_hdr.customer_id
, c.customer_name
, a.mail_address1
, a.mail_address2
, a.mail_city
, a.mail_state
, a.mail_postal_code as postal_code
, oe_hdr.address_id AS ship2_id
, oe_hdr.ship2_name
, oe_hdr.ship2_add1
, oe_hdr.ship2_add2
, oe_hdr.ship2_city
, oe_hdr.ship2_state
, oe_hdr.ship2_zip
, terms_desc
, ac.name as carrier
, oe_hdr.order_date
, oe_hdr.order_no
, oe_hdr.po_no
, inv_mast.item_id
, inv_mast.item_desc
, oe_line.qty_ordered
, CAST(oe_line.qty_on_pick_tickets AS INT) as pick_tickets
, inv_loc.qty_allocated
, oe_line.qty_invoiced
,oe_line.unit_price
, oe_line.disposition
, approved
, ohnote.note
, rep
FROM oe_hdr WITH (NOLOCK)
JOIN p21_view_customer c ON c.customer_id = oe_hdr.customer_id
JOIN p21_view_address a ON c.customer_id = a.id
JOIN p21_view_terms ON p21_view_terms.terms_id = c.terms_id
JOIN oe_line (NOLOCK) ON oe_hdr.order_no = oe_line.order_no
JOIN inv_mast WITH (NOLOCK) ON inv_mast.inv_mast_uid = oe_line.inv_mast_uid
JOIN inv_loc WITH (NOLOCK) ON inv_loc.inv_mast_uid = inv_mast.inv_mast_uid
LEFT OUTER JOIN dbo.DA_Rep ON DA_Rep.customer_id = oe_hdr.customer_id
LEFT OUTER JOIN p21_view_address ac ON ac.id = oe_hdr.carrier_id
JOIN address (NOLOCK) ON address.id = oe_hdr.source_location_id
LEFT OUTER JOIN dbo.p21_view_order_hdr_note ohnote ON (ohnote.order_no = oe_hdr.order_no AND ohnote.area = 'Print Pick Tickets')
WHERE oe_hdr.delete_flag = 'N'
AND oe_line.delete_flag = 'N'
AND oe_line.complete = 'N'
AND inv_loc.location_id = oe_hdr.source_location_id
AND oe_hdr.rma_flag = 'N'
AND oe_hdr.projected_order = 'N'
AND oe_hdr.cancel_flag = 'N'
ORDER BY
oe_hdr.order_date DESC, ship2_name
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
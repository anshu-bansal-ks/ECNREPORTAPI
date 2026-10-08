SELECT oe_hdr.order_no
     , oe_hdr.order_date
     , oe_hdr.customer_id
     , oe_hdr.ship2_name
     , contacts.first_name + ' ' + contacts.last_name AS SalesRepName
     , oe_hdr_salesrep.salesrep_id
     , oe_hdr.po_no
     , inv_mast.item_id
     , inv_mast.item_desc
     , s.preferred_location_id AS default_source
     , oe_line.source_loc_id AS order_source
     , (oe_line.qty_ordered - oe_line.qty_on_pick_tickets - oe_line.qty_invoiced - oe_line.qty_canceled) AS qty_bo
     , inv_loc.qty_on_hand
     {extraSelects}
FROM oe_hdr WITH (NOLOCK)
JOIN oe_hdr_salesrep (NOLOCK) ON oe_hdr.order_no = oe_hdr_salesrep.order_number
JOIN oe_line (NOLOCK) ON oe_hdr.order_no = oe_line.order_no
JOIN inv_mast WITH (NOLOCK) ON inv_mast.inv_mast_uid = oe_line.inv_mast_uid
JOIN p21_view_ship_to s ON s.ship_to_id = oe_hdr.address_id
JOIN inv_loc WITH (NOLOCK) ON inv_loc.inv_mast_uid = inv_mast.inv_mast_uid
LEFT OUTER JOIN V_QTY AS vq (NOLOCK) ON vq.uid = inv_mast.inv_mast_uid
JOIN contacts (NOLOCK) ON oe_hdr_salesrep.salesrep_id = contacts.id
{extraJoins}
WHERE {subQueryRep}
      oe_hdr.delete_flag = 'N'
      AND oe_line.delete_flag = 'N'
      AND oe_line.disposition IN ('B', 'P')
      AND {locationJoinCondition}
      AND oe_hdr.rma_flag = 'N'
      AND oe_hdr.projected_order = 'N'
      AND oe_hdr.cancel_flag = 'N'
      AND oe_hdr.completed = 'N'
      {subQueryStock}
      AND DATEDIFF(dd, oe_hdr.date_created, GETDATE()) <= 730
ORDER BY ship2_name, oe_hdr.order_date ASC
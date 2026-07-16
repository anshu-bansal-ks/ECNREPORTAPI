SELECT key1_value as order_no
, oh.customer_id
, oh.address_id
, oh.ship2_name
, oh.po_no
, dc.name as DEFAULT_CARRIER
, oc.name as CARRIER_USED
, audtr.created_by as changed_by
, audtr.date_created as date_changed
FROM p21_view_audit_trail_oe_hdr_1319 audtr
JOIN dbo.address (NOLOCK) dc ON dc.id = old_value
JOIN address (NOLOCK) oc ON oc.id = audtr.new_value
JOIN p21_view_oe_hdr oh ON oh.order_no = audtr.key1_value
WHERE audtr.column_changed = 'carrier_id'
AND audtr.date_created BETWEEN {dateRange}
AND oh.approved = 'Y'
AND oh.cancel_flag = 'N'
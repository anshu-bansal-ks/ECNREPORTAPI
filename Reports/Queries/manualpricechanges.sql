SELECT oh.order_no
, oh.order_date
, oh.created_by
, oh.projected_order AS quote
, oh.po_no
, oh.customer_id
, oh.ship2_name
, im.item_id
, im.item_desc
, im.price1
, ol.unit_price AS sold_price
, audittrail.old_value AS cust_price
, audittrail.new_value AS over_price
, ol.sales_cost
, audittrail.date_created
, audittrail.created_by AS Audti_created_by 
FROM dbo.p21_view_oe_hdr AS oh
JOIN dbo.p21_view_oe_line AS ol ON ol.oe_hdr_uid = oh.oe_hdr_uid
AND ol.cancel_flag = 'N' AND ol.delete_flag = 'N'
AND ol.manual_price_overide = 'Y' 
JOIN dbo.p21_view_inv_mast AS im ON im.inv_mast_uid = ol.inv_mast_uid
AND im.item_id NOT LIKE 'ADS %' 
JOIN dbo.p21_view_audit_trail_oe_hdr_1319 AS audittrail ON audittrail.key1_value = oh.order_no
AND audittrail.inv_mast_uid = ol.inv_mast_uid
AND audittrail.column_changed = 'UNIT_PRICE'
WHERE audittrail.date_created BETWEEN {dateRange}
AND oh.rma_flag = 'N'
AND oh.cancel_flag = 'N'
AND oh.delete_flag = 'N'
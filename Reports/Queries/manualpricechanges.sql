SELECT oh.order_no
 , oh.order_date
 , oh.created_by
 , oh.projected_order as quote
 , oh.po_no
 , oh.customer_id
 , oh.ship2_name
 , im.item_id
 , im.item_desc
 , im.price1
 , ol.unit_price as sold_price
 , audittrail.old_value as cust_price
 , audittrail.new_value as over_price
 , ol.sales_cost
 , audittrail.date_created
 , audittrail.created_by As Audti_created_by
 FROM p21_view_oe_hdr oh
 JOIN dbo.p21_view_oe_line ol ON ol.oe_hdr_uid = oh.oe_hdr_uid
 JOIN dbo.p21_view_inv_mast im ON im.inv_mast_uid = ol.inv_mast_uid
 JOIN p21_view_audit_trail_oe_hdr_1319 audittrail ON audittrail.key1_value = oh.order_no
 AND audittrail.inv_mast_uid = ol.inv_mast_uid
 AND column_changed = 'UNIT_PRICE'
 WHERE oh.order_date BETWEEN {dateRange}
 AND ol.manual_price_overide = 'Y'
 AND oh.rma_flag = 'N'
 AND oh.cancel_flag = 'N'
 and ol.cancel_flag = 'N'
 AND oh.delete_flag = 'N'
 AND ol.delete_flag = 'N'
 AND im.item_id NOT LIKE 'ADS %'
  SELECT ih.invoice_no
  , ih.order_no
  , oh.taker
  , oh.job_name
  , ih.po_no
  , ih.invoice_date
  , ih.customer_id
  , ih.bill2_name
  , ih.ship_to_id
  , ih.ship2_name
  , ih.ship2_state as state
  , il.item_id
  , il.item_desc
  , il.qty_requested
  , il.qty_shipped
  , il.unit_price as price
  , il.extended_price
  , CASE WHEN s.preferred_location_id = 100035 THEN 'NJ'
  WHEN s.preferred_location_id = 100038 THEN 'FL'
  WHEN s.preferred_location_id = 100105 THEN 'CA' ELSE 'ERROR'
  END AS Def_Loc
  , CASE WHEN pt.location_id = 100035 THEN 'NJ'
  WHEN pt.location_id = 100038 THEN 'FL'
  WHEN pt.location_id = 100105 THEN 'CA' ELSE 'ERROR'
  END AS Used_loc
  , q.NJ_QTY as jn
  , q.FL_Qty as fl
  , q.CA_Qty as ca
  FROM p21_view_invoice_hdr ih
  JOIN dbo.p21_view_invoice_line il ON il.invoice_no = ih.invoice_no
  JOIN p21_view_ship_to s ON s.ship_to_id = ih.ship_to_id
  JOIN p21_view_oe_pick_ticket pt ON pt.invoice_no = ih.invoice_no
  JOIN p21_view_oe_hdr oh ON oh.order_no = pt.order_no
  JOIN p21_view_customer c ON c.customer_id = ih.customer_id
  JOIN v_qty q ON q.uid = il.inv_mast_uid
  JOIN p21_view_codes codes ON codes.code_no = oh.source_code_no AND codes.code_group_no = 1012
  WHERE invoice_date BETWEEN {dateRange} 
  AND pt.location_id != s.preferred_location_id
  AND ih.rma_flag = 'N'
  AND ( c.class_1id != 'ADS'
  OR c.class_1id IS NULL )
  Order By ih.customer_id
  OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
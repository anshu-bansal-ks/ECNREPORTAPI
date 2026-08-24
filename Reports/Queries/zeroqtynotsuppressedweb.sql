SELECT item_id
, item_desc
, release_date
, default_sales_discount_group as discount_group
, Last_Sold
, Last_Bought
, NJ_QTY
, FL_Qty
, CA_Qty
, Tot_Qty as total_Qty
, CAST(Tot_on_Order AS INT) as total_order
, nj_discontinued
, fl_discontinued
, ca_discontinued
, suppress_web
, suppress_feed
FROM v_item_flags_stats
WHERE 1=1
AND Tot_Qty=0
AND release_date<GETDATE ()
AND Tot_on_Order=0
AND (ISNULL(@discontinued, 'false') = 'false' OR (nj_discontinued = 'Y' 
AND ca_discontinued = 'Y' AND fl_discontinued = 'Y'))
AND suppress_web='N'
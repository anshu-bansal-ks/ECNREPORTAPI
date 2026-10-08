SELECT suppress_web
, suppress_feed
, inv_mast_uid
, item_id
, item_desc
, release_date
, Last_Sold
, default_sales_discount_group as discount_group
, Last_Bought
, Tot_Qty
, Tot_on_Order
, nj_buy
, fl_buy
, ca_buy
, nj_sellable
, fl_sellable
, ca_sellable
, nj_discontinued
, fl_discontinued
, ca_discontinued
, NJ_ABC
, FL_ABC
, CA_ABC
FROM v_item_flags_stats
WHERE nj_discontinued='Y'
AND fl_discontinued='Y'
AND ca_discontinued='Y'
AND Tot_Qty=0
AND GETDATE () >release_date
AND (suppress_web = 'N' OR suppress_web IS NULL OR suppress_feed = 'N' or suppress_feed IS NULL)
AND Tot_on_Order = 0
ORDER BY item_id
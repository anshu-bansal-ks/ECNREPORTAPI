SELECT im.item_id
, im.item_desc
, im.parker_product_cd as parker_product
, im.date_created
, ud.release_date
FROM p21_view_inv_mast (NOLOCK) im
JOIN p21_view_inv_loc (NOLOCK) il ON (
il.inv_mast_uid = im.inv_mast_uid
AND il.location_id = 100035
AND il.buy = 'Y'
AND il.sellable = 'Y'
AND il.delete_flag = 'N'
)
LEFT OUTER JOIN inv_mast_ud (NOLOCK) ud ON ud.inv_mast_uid = im.inv_mast_uid
WHERE im.delete_flag = 'N'
AND im.other_charge_item = 'N'
AND im.parker_product_cd IS NULL
SELECT p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, primary_bin
, inv_mast_ud.suppress_from_web
FROM p21_view_inv_mast (NOLOCK)
JOIN p21_view_inv_loc (NOLOCK) ON p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid
LEFT JOIN inv_mast_ud (NOLOCK) ON inv_mast_ud.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
WHERE (
(p21_view_inv_mast.item_desc LIKE '%(C)%')
AND (p21_view_inv_loc.location_id = $100002)
AND (p21_view_inv_loc.qty_on_hand = 0)
)
ORDER BY p21_view_inv_mast.item_id
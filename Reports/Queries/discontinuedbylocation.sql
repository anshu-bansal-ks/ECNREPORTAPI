SELECT p21_view_inv_mast.item_id, 
p21_view_inv_mast.item_desc, 
p21_view_inv_loc.primary_bin,
p21_view_inv_loc.location_id, 
p21_view_inv_loc.discontinued
FROM p21_view_inv_loc AS p21_view_inv_loc 
INNER JOIN p21_view_inv_mast AS p21_view_inv_mast ON p21_view_inv_loc.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
WHERE p21_view_inv_mast.item_desc LIKE 'no longer%'
AND p21_view_inv_loc.primary_bin <> '00'
AND p21_view_inv_mast.delete_flag = 'n'
AND p21_view_inv_loc.location_id = @locationId
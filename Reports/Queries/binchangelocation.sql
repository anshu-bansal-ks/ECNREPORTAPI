SELECT p21_view_inv_mast.item_id
, dbo.p21_view_inv_mast.item_desc
, p21_view_inv_loc.stockable
, CAST(dbo.p21_view_inv_loc.qty_on_hand AS INT) as qty_on_hand
, p21_view_inv_loc.primary_bin
, p21_view_inv_bin.bin
FROM p21_view_inv_mast WITH (NOLOCK)
JOIN p21_view_inv_loc WITH (NOLOCK) ON p21_view_inv_loc.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
JOIN p21_view_inv_bin WITH (NOLOCK) ON p21_view_inv_bin.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
WHERE p21_view_inv_loc.location_id = @locationId
AND dbo.p21_view_inv_bin.location_id = p21_view_inv_loc.location_id
AND (ISNULL(@stockable, 'false') = 'false' OR p21_view_inv_loc.stockable='Y')
AND p21_view_inv_loc.primary_bin <> p21_view_inv_bin.bin
SELECT {topsub} 
ROW_NUMBER() OVER(ORDER BY(SELECT 1)) AS sr_No,
tbl_productrank.item_id,
tbl_productrank.item_desc,
ISNULL(v_upc.upc, '') as upc,
price1,
tbl_productrank.product_group_desc as product_group, 
tbl_productrank.supplier_name,
slf.mcat,
slf.scat,
CAST( tbl_productrank.slqty AS INT) AS slqty
FROM tbl_productrank WITH (nolock) 
LEFT OUTER JOIN v_barcode WITH (NOLOCK) ON v_barcode.inv_mast_uid = tbl_productrank.inv_mast_uid
Inner join {dashboard}.dbo.salsify_itemData slf on slf.item_id = tbl_productrank.item_id
JOIN p21_view_inv_mast im ON im.inv_mast_uid = tbl_productrank.inv_mast_uid
LEFT OUTER JOIN v_upc ON v_upc.inv_mast_uid = tbl_productrank.inv_mast_uid
WHERE tbl_productrank.supplier_id = @supplierId
AND (@mcat = '' OR slf.mcat = @mcat)
AND (@scat = '' OR slf.scat = @scat)
ORDER BY tbl_productrank.slqty DESC,
tbl_productrank.item_id

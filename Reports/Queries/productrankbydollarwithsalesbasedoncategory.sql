SELECT {topsub}
ROW_NUMBER() OVER(ORDER BY(SELECT 1)) AS sr_No,
tbl_productrankdollar.item_id,
tbl_productrankdollar.item_desc,
tbl_productrankdollar.product_group_desc as product_group, 
tbl_productrankdollar.supplier_name,
slf.mcat,
slf.scat,
tbl_productrankdollar.sales
FROM tbl_productrankdollar WITH (nolock) 
LEFT OUTER JOIN v_barcode WITH (NOLOCK) ON v_barcode.inv_mast_uid = tbl_productrankdollar.inv_mast_uid
Inner join {dashboard}.dbo.salsify_itemData slf on slf.item_id = tbl_productrankdollar.item_id
Where tbl_productrankdollar.supplier_id = @supplierId
AND (@mcat = '' OR slf.mcat = @mcat)
AND (@scat = '' OR slf.scat = @scat)
ORDER BY tbl_productrankdollar.sales DESC,
tbl_productrankdollar.item_id
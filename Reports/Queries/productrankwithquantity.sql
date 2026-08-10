SELECT {topsub}
ROW_NUMBER() OVER(ORDER BY(SELECT 1)) AS sr_No,
tbl_productrank.item_id,
tbl_productrank.item_desc,
tbl_productrank.product_group_desc as product_group, 
tbl_productrank.supplier_name,
CAST( tbl_productrank.slqty AS INT) AS slqty
FROM tbl_productrank WITH (nolock) 
LEFT OUTER JOIN v_barcode WITH (NOLOCK) ON v_barcode.inv_mast_uid = tbl_productrank.inv_mast_uid
Where 1=1
AND (NULLIF(@supplierId, '') IS NULL
     OR tbl_productrank.supplier_id = CAST(@supplierId AS INT))
And(@productgroup = '' or tbl_productrank.product_group_id = @productgroup)
ORDER BY tbl_productrank.slqty DESC,
tbl_productrank.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
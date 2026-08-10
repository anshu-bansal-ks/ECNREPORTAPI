SELECT {topsub}
ROW_NUMBER() OVER(ORDER BY(SELECT 1)) AS sr_No,
tbl_productrankdollar.item_id, 
tbl_productrankdollar.item_desc, 
tbl_productrankdollar.product_group_desc as product_group, 
tbl_productrankdollar.supplier_name, 
tbl_productrankdollar.sales 
FROM tbl_productrankdollar WITH (nolock)  
LEFT OUTER JOIN v_barcode WITH (NOLOCK) ON v_barcode.inv_mast_uid = tbl_productrankdollar.inv_mast_uid 
Where 1=1
AND (NULLIF(@supplierId, '') IS NULL
     OR tbl_productrankdollar.supplier_id = CAST(@supplierId AS INT))
And(@productgroup = '' or tbl_productrankdollar.product_group_id = @productgroup)
ORDER BY tbl_productrankdollar.sales DESC,
tbl_productrankdollar.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
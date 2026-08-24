SELECT {topsub} ROW_NUMBER() OVER ( ORDER BY 
CASE WHEN @rankType = 'QTY' THEN ISNULL(tbl_productrank.slqty, 0)
ELSE ISNULL(sales.sales_6month, 0) END DESC, tbl_productrank.item_id ) AS sr_No,
tbl_productrank.item_id,
dbo.v_barcode.upc,
tbl_productrank.item_desc,
im.price1 as price,
tbl_productrank.product_group_desc as product_group,
tbl_productrank.supplier_name
FROM tbl_productrank WITH (NOLOCK)
LEFT JOIN ( SELECT il.item_id,
SUM(il.extended_price) AS sales_6month
FROM invoice_hdr ih WITH (NOLOCK)
INNER JOIN invoice_line il WITH (NOLOCK) ON ih.invoice_no = il.invoice_no
WHERE ih.invoice_date >= DATEADD(MONTH, -6, GETDATE())
GROUP BY il.item_id
) sales ON sales.item_id = tbl_productrank.item_id
LEFT OUTER JOIN v_barcode WITH (NOLOCK) ON v_barcode.inv_mast_uid = tbl_productrank.inv_mast_uid
JOIN p21_view_inv_mast im ON im.inv_mast_uid = tbl_productrank.inv_mast_uid
WHERE 1= 1
AND (NULLIF(@supplierId, '') IS NULL
OR tbl_productrank.supplier_id = CAST(@supplierId AS INT))
And(@productgroup = '' or tbl_productrank.product_group_id = @productgroup)
ORDER BY CASE WHEN @rankType = 'QTY'
THEN ISNULL(tbl_productrank.slqty, 0)
ELSE ISNULL(sales.sales_6month, 0) END DESC,
tbl_productrank.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
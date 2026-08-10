SELECT il.item_id
, im.item_desc
, il.supplier_id
, s.supplier_name
, slf.mcat
, slf.scat
, SUM (il.qty_shipped) as qty
, SUM (il.extended_price) as SALES
FROM invoice_hdr ih(NOLOCK)
JOIN invoice_line(NOLOCK) il ON ih.invoice_no=il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs(NOLOCK)ON ih.invoice_no=ihs.invoice_number
JOIN inv_mast(NOLOCK) im ON im.inv_mast_uid=il.inv_mast_uid
JOIN dbo.p21_view_supplier s ON il.supplier_id=s.supplier_id
Inner join {dashboard}.dbo.salsify_itemData slf on slf.item_id = il.item_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND il.item_id IN( SELECT DISTINCT
modelid
FROM {dashboard}.dbo.lnkItemCategories )
AND il.supplier_id IS NOT NULL
AND (@mcat = '' OR slf.mcat = @mcat)
AND (@scat = '' OR slf.scat = @scat) 
GROUP BY il.item_id
, im.item_desc
, il.supplier_id
, s.supplier_name
, slf.mcat
, slf.scat
ORDER BY il.item_id
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
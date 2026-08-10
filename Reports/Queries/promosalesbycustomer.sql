SELECT c.customer_id
, c.customer_name
, rep
, SUM (il.extended_price) as SALES
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
JOIN p21_view_customer c ON c.customer_id = ih.customer_id
JOIN da_rep (nolock)  ON da_rep.customer_id = ih.customer_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND il.item_id IN (
SELECT ItemId FROM {dashboard}.dbo.PromosItems (NOLOCK) si WHERE PromoId = @PromoId
)
AND ( @AllPO = 'true' OR @pono = '' OR ih.po_no LIKE '%' + @pono + '%')
GROUP BY c.customer_id
, c.customer_name,rep
ORDER BY c.customer_name
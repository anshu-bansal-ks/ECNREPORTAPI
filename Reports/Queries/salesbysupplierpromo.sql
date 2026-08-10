SELECT a.supplier_id
, a.supplier_name
, SALES
, ISNULL(featured.FEATURED_SALES,0) as FEATURED_SALES
, SALES - ISNULL(FEATURED.FEATURED_SALES, 0) as other_sales
FROM ( SELECT s.supplier_id
, s.supplier_name
, SUM(il.extended_price) SALES
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
JOIN dbo.invoice_hdr_salesrep ihs (NOLOCK) ON ih.invoice_no = ihs.invoice_number
JOIN contacts c (NOLOCK) ON c.id = ihs.salesrep_id
JOIN inv_mast (NOLOCK) im ON im.inv_mast_uid = il.inv_mast_uid
JOIN supplier (NOLOCK) s ON s.supplier_id = il.supplier_id
Inner Join( SELECT distinct
supplier_id
FROM v_supplier_x_loc s 
inner join {dashboard}.dbo.PromosItems (NOLOCK) si on si.ItemId = s.item_id
WHERE location_id = 100035 and PromoId = @PromoId
)as ss on ss.supplier_id = s.supplier_id
WHERE ih.invoice_date BETWEEN {dateRange}
AND ihs.primary_salesrep = 'Y'
AND ( @AllPO = 'true' OR @pono = '' OR ih.po_no LIKE '%' + @pono + '%')
GROUP BY s.supplier_id
, s.supplier_name
)a
LEFT OUTER JOIN (
SELECT il.supplier_id
, SUM(il.extended_price) FEATURED_SALES
FROM invoice_hdr ih (NOLOCK)
JOIN invoice_line (NOLOCK) il ON ih.invoice_no = il.invoice_no
INNER JOIN {dashboard}.dbo.PromosItems (NOLOCK) si on si.ItemId = il.item_id and PromoId = @promoid
WHERE ih.invoice_date BETWEEN {dateRange}
AND ( @AllPO = 'true' OR @pono = '' OR ih.po_no LIKE '%' + @pono + '%')
GROUP BY il.supplier_id ) AS FEATURED ON FEATURED.supplier_id = a.supplier_id
ORDER BY a.supplier_name
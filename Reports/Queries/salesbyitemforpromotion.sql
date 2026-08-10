select im.item_id
, im.item_desc
, s.supplier_id
, s.supplier_name
, CAST(sum(il.qty_shipped) AS INT) as UNITS
, sum(il.extended_price)as SALES
, sum(il.cogs_amount) as COST
, sum(il.extended_price) - sum(il.cogs_amount) as gross_profit
, case when sum(il.extended_price) = 0 then 0 else
round((sum(il.extended_price) - sum(il.cogs_amount)) / sum(il.extended_price) * 100, 2) end profit_percent
from invoice_hdr ih (nolock)
join invoice_line (nolock) il on ih.invoice_no = il.invoice_no
join inv_mast (nolock) im on im.inv_mast_uid = il.inv_mast_uid
join supplier (nolock) s on s.supplier_id = il.supplier_id
where ih.invoice_date BETWEEN {dateRange}
and il.item_id in(
select ItemId from {dashboard}.dbo.PromosItems (nolock) si where PromoId = @promoId
)
AND (
    @AllPO = 'true'
    OR @pono = ''
    OR ih.po_no LIKE '%' + @pono + '%'
)
group by im.item_id
, im.item_desc
, s.supplier_id
, s.supplier_name
order by im.item_id
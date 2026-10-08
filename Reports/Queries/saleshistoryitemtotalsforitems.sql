WITH Item_CTE (item_id, item_desc, UNITS, SALES) As (
    SELECT im.item_id,
    im.item_desc,
    Sum(qty_shipped) UNITS,
    Sum(il.extended_price) SALES
    FROM invoice_hdr ih (nolock)
    JOIN invoice_line (nolock) il ON ih.invoice_no = il.invoice_no
    JOIN dbo.invoice_hdr_salesrep ihs (nolock) ON ih.invoice_no = ihs.invoice_number
    JOIN contacts c (nolock) ON c.id = ihs.salesrep_id
    JOIN inv_mast (nolock) im ON im.inv_mast_uid = il.inv_mast_uid
    JOIN supplier (nolock) s ON s.supplier_id = il.supplier_id
    RIGHT JOIN {dashboard}.dbo.fn_CommaSeparatedStringToTable(@ItemIdList, 'N') tbl on tbl.Value = il.item_id
    WHERE {dateRange}
    AND ihs.primary_salesrep = 'Y'
    GROUP BY im.item_id, im.item_desc
)
SELECT lst.value As item_id,
case when im.item_desc is null then 'Item Not Found' else im.item_desc end as item_desc,
UNITS, SALES
from Item_CTE tbl
RIGHT Join {dashboard}.dbo.fn_CommaSeparatedStringToTable(@ItemIdList, 'N') lst on lst.Value = tbl.item_id
Left join inv_mast im on im.item_id = lst.value
select p21_view_inv_mast.item_id
, p21_view_inv_mast.item_desc
, CAST((p21_view_inv_loc.qty_on_hand - qty_allocated) AS INT) AS qty_available
, p21_view_inv_mast.price1
, p21_view_inv_mast.price8
, p21_view_inv_loc.sales_discount_group
, dbo.v_barcode.upc
from p21_view_inv_loc (nolock)
join p21_view_inv_mast (nolock) on p21_view_inv_loc.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
left outer join dbo.v_barcode (nolock) on v_barcode.inv_mast_uid = p21_view_inv_mast.inv_mast_uid
where p21_view_inv_loc.location_id = @locationId
and p21_view_inv_mast.delete_flag = 'n'
and sales_discount_group = 'CLOSEOUT'
and p21_view_inv_loc.price8 > 0
and p21_view_inv_loc.qty_on_hand - qty_allocated >= @minavail
order by p21_view_inv_mast.item_id
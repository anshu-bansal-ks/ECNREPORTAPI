SELECT s.supplier_name
, inv_mast.item_id
, inv_mast.item_desc
, isnull(v_upc.upc,'') as upc
, inv_mast.price1
, inv_mast_ud.release_date
, inv_mast.date_created
, inv_mast.created_by
, inv_mast.last_maintained_by
, inv_mast_ud.suppress_web
, inv_mast_ud.suppress_feed
FROM p21_view_inv_mast inv_mast
JOIN dbo.DA_SUPPLIER_ID ON DA_SUPPLIER_ID.inv_mast_uid=inv_mast.inv_mast_uid
JOIN p21_view_supplier s ON s.supplier_id = DA_SUPPLIER_ID.supplier_id
LEFT OUTER JOIN inv_mast_ud (NOLOCK)ON inv_mast_ud.inv_mast_uid=inv_mast.inv_mast_uid
left outer join v_upc on v_upc.inv_mast_uid = inv_mast.inv_mast_uid
WHERE inv_mast.date_created BETWEEN {dateRange}  
ORDER BY supplier_name, item_id
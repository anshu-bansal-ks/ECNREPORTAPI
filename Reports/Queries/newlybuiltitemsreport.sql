  SELECT s.supplier_name
  , inv_mast.item_id
  , inv_mast.item_desc
  , inv_mast_ud.release_date
  , inv_mast.date_created
  , inv_mast.created_by
  , inv_mast.last_maintained_by
  FROM p21_view_inv_mast (NOLOCK) inv_mast
  JOIN dbo.DA_SUPPLIER_ID (NOLOCK) ON DA_SUPPLIER_ID.inv_mast_uid=inv_mast.inv_mast_uid
  JOIN p21_view_supplier (NOLOCK) s ON s.supplier_id = DA_SUPPLIER_ID.supplier_id
  LEFT OUTER JOIN inv_mast_ud (NOLOCK) ON inv_mast_ud.inv_mast_uid=inv_mast.inv_mast_uid
  WHERE inv_mast.date_created BETWEEN {dateRange}  
  ORDER BY supplier_name, item_id
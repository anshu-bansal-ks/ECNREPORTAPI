SELECT {topsub} pr.[item_id]
, im.[item_desc] as item_description
, im.price1 as price
, (2 * im.price1) AS MSRP
, invsup.cost as supplier_cost
, CAST(il.qty_on_hand-il.qty_allocated AS INT) as qty_on_hand
, pr.[product_group_desc] as product_group
, pr.[supplier_name]
FROM [tbl_productrank] pr
JOIN p21_view_inv_mast im ON im.inv_mast_uid = pr.inv_mast_uid
JOIN p21_view_inv_loc il ON il.inv_mast_uid = im.inv_mast_uid
AND il.location_id = 100002
JOIN dbo.p21_view_inventory_supplier invsup ON invsup.inv_mast_uid = pr.inv_mast_uid
AND invsup.supplier_id = pr.supplier_id
Where 1 = 1
AND (NULLIF(@supplierId, '') IS NULL
     OR pr.supplier_id = CAST(@supplierId AS INT))
And(@productgroup = '' or pr.product_group_id = @productgroup)
ORDER BY pr.slqty DESC
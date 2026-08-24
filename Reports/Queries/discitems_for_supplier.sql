SELECT im.item_id    
,im.item_desc  
,s.supplier_name 
,im.price1  
{subQuery}     
,CAST(vq.Tot_Qty AS INT ) as Total    
,il.standard_cost as cost
,(il.standard_cost * vq.Tot_Qty) as 'value'
FROM p21_view_inv_mast im
JOIN p21_view_inv_loc il ON il.inv_mast_uid = im.inv_mast_uid
JOIN v_supplier_x_loc vsxl ON ( vsxl.item_id = il.item_id 
AND vsxl.location_id = il.location_id )                   
JOIN p21_view_inventory_supplier invsup ON invsup.inv_mast_uid = im.inv_mast_uid
JOIN p21_view_supplier s ON s.supplier_id = vsxl.supplier_id
JOIN dbo.V_QTY AS vq ON vq.UID = im.inv_mast_uid
WHERE il.location_id = @locationId 
AND im.item_desc LIKE '%DISC%'
AND s.supplier_id = @supplierId 
AND vq.Tot_Qty > 0
ORDER BY im.item_id
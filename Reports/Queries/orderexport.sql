SELECT oe_line.order_no  
,inv_mast.item_id  
,inv_mast.price1   
,oe_line.unit_price
,round(((inv_mast.price1-unit_price)/NULLIF(inv_mast.price1,0))*100,2) as Discount
,inv_mast.item_desc  
,CAST(oe_line.qty_ordered AS INT ) as qty_ordered
,ISNULL(v_upc.upc, '') AS upc          
FROM oe_hdr (NOLOCK)      
JOIN oe_line (NOLOCK) ON dbo.oe_hdr.order_no = dbo.oe_line.order_no
JOIN inv_mast (NOLOCK) ON dbo.oe_line.inv_mast_uid = dbo.inv_mast.inv_mast_uid
LEFT OUTER JOIN V_upc (NOLOCK) ON dbo.inv_mast.inv_mast_uid = dbo.v_upc.inv_mast_uid
WHERE oe_line.order_no = @ordernum
AND oe_line.delete_flag = 'N'
GROUP BY oe_line.order_no  
,inv_mast.item_id  
,inv_mast.price1   
,oe_line.unit_price
,(((inv_mast.price1-unit_price)/NULLIF(unit_price,0))*100)
,inv_mast.item_desc  
,oe_line.qty_ordered 
,v_upc.upc           
ORDER BY inv_mast.item_id
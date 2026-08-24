SELECT inv_mast.item_id
,item_desc
,v_upc.upc
{subQuery}
,CAST(vq.Tot_Qty AS INT) as total
FROM inv_mast  (NOLOCK)
LEFT JOIN v_upc  (NOLOCK) ON v_upc.inv_mast_uid = inv_mast.inv_mast_uid
JOIN dbo.V_QTY AS vq ( NOLOCK ) ON vq.UID = inv_mast.inv_mast_uid 
WHERE inv_mast.delete_flag = 'N' 
and inv_mast.item_id LIKE  @prefix + '%'  
ORDER BY inv_mast.item_id
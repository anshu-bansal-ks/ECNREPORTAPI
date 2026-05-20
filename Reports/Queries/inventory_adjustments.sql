SELECT hdr.adjustment_number
,hdr.date_created 
,hdr.location_id
,location_name
,item_id     
,item_desc   
,CAST(unit_quantity AS INT) as unit_qty
,hdr.last_maintained_by as last_maintained
,reason
FROM dbo.inv_adj_line (NOLOCK) line
JOIN inv_adj_hdr (NOLOCK) hdr ON line.adjustment_number = hdr.adjustment_number
JOIN inv_mast (NOLOCK) ON inv_mast.inv_mast_uid = line.inv_mast_uid
JOIN reason (NOLOCK) ON reason.id = hdr.reason_id
JOIN dbo.location (NOLOCK) ON hdr.location_id = dbo.location.location_id
WHERE hdr.date_created BETWEEN {dateRange}
AND hdr.delete_flag = 'N'
AND hdr.approved = 'Y'
AND (line.unit_quantity >= @minqty 
OR line.unit_quantity <= (-@minqty))
ORDER BY hdr.date_created
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
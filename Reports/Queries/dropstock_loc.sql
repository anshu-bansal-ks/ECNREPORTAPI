DECLARE @binprefix varchar(10)
DECLARE @bin2 VARCHAR(11)              
declare @loc int                  
SET @binprefix =@bin                   
SET @bin2 = @binprefix + '%'           
SET @loc =@locationId   
SELECT p21_view_inv_loc.primary_bin   
,p21_view_inv_loc.item_id       
,p21_view_inv_mast.item_desc    
,CAST(p21_view_inv_loc.qty_on_hand AS INT) as qty_on_hand
,CAST(p21_view_inv_loc.qty_allocated AS INT) as qty_allocated
,CAST(p21_view_inv_loc.order_quantity AS INT) order_quantity
FROM p21_view_inv_loc (NOLOCK) JOIN
p21_view_inv_mast (NOLOCK)  ON  p21_view_inv_mast.inv_mast_uid = p21_view_inv_loc.inv_mast_uid 
WHERE p21_view_inv_loc.location_id = @loc  
AND  p21_view_inv_loc.primary_bin LIKE @bin2 
AND p21_view_inv_mast.delete_flag = 'N'
order by primary_bin
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;
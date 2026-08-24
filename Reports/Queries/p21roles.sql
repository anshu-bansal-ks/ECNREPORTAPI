SELECT p21_view_users.id as user_id
,p21_view_users.name as user_name
,p21_view_roles.role
,p21_view_users.email_address
FROM p21_view_roles(nolock) 
join p21_view_users(nolock) on p21_view_users.role_uid = p21_view_roles.role_uid 
WHERE p21_view_users.delete_flag='N'
AND( @roles = 'ALL_ROLE' OR CAST(p21_view_roles.role_uid AS VARCHAR(50)) = @roles) 

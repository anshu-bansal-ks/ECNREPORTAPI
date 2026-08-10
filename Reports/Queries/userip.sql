Select Username as user_name
,LoginUserIP as login_user_ip
,max(LoginDateTime) as last_Login  
from {dashboard}.dbo.UPortalLoginUserHistory 
where Username !='administrator'
group by Username
,LoginUserIP  
order by 1,3 desc
SELECT role_name
,report_name
FROM {dashboard}.dbo.Roles (nolock) rs
join {dashboard}.dbo.Role_Report (NOLOCK) rn  ON rs.RoleId = rn.RoleId 
join {dashboard}.dbo.Reports (NOLOCK) re  ON rn.ReportId = re.ReportId
where 1 =1
AND( @rolesreports = 'ALL' OR CAST(rs.RoleId AS VARCHAR(50))  = @rolesreports) 
ORDER BY role_name

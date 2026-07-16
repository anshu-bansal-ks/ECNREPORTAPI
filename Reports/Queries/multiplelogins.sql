SELECT DISTINCT loginame as login_name
,nt_username
,DB_NAME(dbid) as database_name
,dbid
,hostname as host_name
FROM master.dbo.sysprocesses
WHERE loginame LIKE 'IVD1\%'
AND program_name LIKE '%PXXI%'
AND nt_username NOT IN ( 'allocater', 'importer' )
AND DB_NAME(dbid) != 'TEMPDB'
AND loginame IN (
SELECT users.loginame
FROM ( SELECT loginame
,nt_username
,DB_NAME(dbid) database_name
FROM master.dbo.sysprocesses
WHERE loginame LIKE 'IVD1\%'
AND program_name LIKE '%PXXI%'
AND nt_username NOT IN ( 'allocater', 'importer','xgimporter' )
AND DB_NAME(dbid) != 'TEMPDB'
GROUP BY  loginame
,nt_username
,DB_NAME(dbid)
) AS users
GROUP BY users.loginame
,users.nt_username
HAVING  COUNT(users.database_name) >= @numb )
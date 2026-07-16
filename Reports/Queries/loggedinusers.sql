Select * from (SELECT DISTINCT
sysprocesses.loginame as login_name
,sysprocesses.nt_username
,(DB_NAME(sysprocesses.dbid)) as database_name
,dbid
,convert(varchar(40),GETDATE(),25) as [current_date]
,CAST(sysprocesses.login_time AS SMALLDATETIME) as date_logged_in
,client_net_address
,sysprocesses.hostname as host_name
FROM master.dbo.sysprocesses
INNER JOIN master.sys.dm_exec_connections ec ON sysprocesses.spid = ec.session_id
WHERE sysprocesses.loginame LIKE 'IVD1%'
AND sysprocesses.program_name LIKE '%PXXI%'
AND sysprocesses.nt_username NOT IN ( 'allocater', 'importer',
'xgimporter', 'baciimporter', 'APIUSER' )
AND DB_NAME(sysprocesses.dbid) NOT IN ('TEMPDB', 'ecn_dev')
GROUP BY sysprocesses.loginame
,sysprocesses.nt_username
,DB_NAME(sysprocesses.dbid)
,sysprocesses.dbid
,sysprocesses.cmd
,CAST(sysprocesses.login_time AS SMALLDATETIME)
,client_net_address
,sysprocesses.hostname ) t
ORDER BY date_logged_in ASC
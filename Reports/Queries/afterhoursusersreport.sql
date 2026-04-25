SELECT [Loginname]
,[nt_username]
,[database_name]
,[dbid]
,[datadate]
,[dateloggedin]
,[client_net_address]
,[hostname]
FROM {dashboard}.dbo.dailyuserloggedindetails WITH (NOLOCK)
WHERE hostname NOT IN ('ECNP21SCHED')
AND datadate BETWEEN {dateRange}
ORDER BY datadate DESC, Loginname
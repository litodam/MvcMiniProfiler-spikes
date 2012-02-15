namespace MvcMiniProfilerSample.Controllers
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Web.Mvc;
    using Dapper;

    public class DashboardController : Controller
    {
        // Change this to point to your SQL Server
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ProductsContext"].ConnectionString;

        public ActionResult Index()
        {
            const string sql =
                @"select SRC.Name as WebRoute, count(SRC.name) as Samples, avg(DurationMilliseconds) as AvgD, min(DurationMilliseconds) as Low, max(DurationMilliseconds) as High, max(Ranks.Under10) as LowSample, max(Ranks.Over90) as HighSample, max(LowRanks.LongestDuration) as BoxLow, max(HighRanks.LongestDuration) as BoxHigh
from
(
	select Name,
		DurationMilliseconds,
		Dense_Rank() over (partition by Name order by DurationMilliseconds) as drank
	from MiniProfilers
) AS src
LEFT OUTER JOIN (
	select Name, floor( (max(src2.drank) - min(src2.drank)) * 0.25 ) + 1 as Under10, ceiling( (max(src2.drank) - min(src2.drank)) * 0.75 ) + 1 as Over90
	from
	(
		select Name,
			DurationMilliseconds,
			Dense_Rank() over (partition by Name order by DurationMilliseconds) as drank
		from MiniProfilers
	) AS SRC2
	group by name
) AS Ranks ON Src.Name = Ranks.Name
LEFT OUTER JOIN (
	select Name,
		DurationMilliseconds as LongestDuration,
		Dense_Rank() over (partition by Name order by DurationMilliseconds) as drank
	from MiniProfilers
	group by name, DurationMilliseconds
) AS LowRanks ON Src.Name = LowRanks.Name AND Ranks.Under10 = LowRanks.drank
LEFT OUTER JOIN (
	select Name,
		DurationMilliseconds as LongestDuration,
		Dense_Rank() over (partition by Name order by DurationMilliseconds) as drank
	from MiniProfilers
	group by name, DurationMilliseconds
) AS HighRanks ON Src.Name = HighRanks.Name AND Ranks.Over90 = HighRanks.drank
group by SRC.Name
order by BoxHigh DESC;";

            IEnumerable<dynamic> data;

            using (DbConnection conn = GetOpenConnection())
            {                 
                data = conn.Query(sql);
            }

            return View(data);
        }

        /// <summary>
        /// 	Returns a connection to Sql Server.
        /// </summary>
        protected DbConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// 	Returns a DbConnection already opened for execution.
        /// </summary>
        protected DbConnection GetOpenConnection()
        {
            DbConnection result = GetConnection();
            if (result.State != ConnectionState.Open)
            {
                result.Open();
            }
            return result;
        }
    }
}
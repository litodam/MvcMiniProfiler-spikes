[assembly: WebActivator.PreApplicationStartMethod(typeof(MvcMiniProfilerSample.MiniProfilerInitialization), "PreStart")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(MvcMiniProfilerSample.MiniProfilerInitialization), "PostStart")]

namespace MvcMiniProfilerSample
{
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using MvcMiniProfiler;
    using MvcMiniProfiler.MVCHelpers;
    using MvcMiniProfiler.Storage;

    public static class MiniProfilerInitialization
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["ProductsContext"].ConnectionString;

        public static void PreStart()
        {
            MiniProfiler.Settings.SqlFormatter = new MvcMiniProfiler.SqlFormatters.SqlServerFormatter();
            
            // using SqlServerStorage provider instead of default
            MiniProfiler.Settings.Storage = new SqlServerStorage(connectionString);                                    

            // var sqlConnectionFactory = new SqlConnectionFactory(ConfigurationManager.ConnectionStrings["ProductsContext"].ConnectionString);
            // var profiledConnectionFactory = new ProfiledDbConnectionFactory(sqlConnectionFactory);
            // Database.DefaultConnectionFactory = profiledConnectionFactory;
            MiniProfilerEF.Initialize();

            DynamicModuleUtility.RegisterModule(typeof(MiniProfilerStartupModule));
            GlobalFilters.Filters.Add(new ProfilingActionFilter());
        }

        public static void PostStart()
        {
            var viewEngines = ViewEngines.Engines.ToList();

            ViewEngines.Engines.Clear();

            foreach (var item in viewEngines)
            {
                ViewEngines.Engines.Add(new ProfilingViewEngine(item));
            }
        }

        public static void InitializeMiniProfilerSqlStorage()
        {            
            var conn = new SqlConnection(connectionString);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using (conn)
            {
                // Create and execute the DbCommand.
                DbCommand command = conn.CreateCommand();
                command.CommandText = SqlServerStorage.TableCreationScript;                    
                int rows = command.ExecuteNonQuery();
            }            
        }
    }

    public class MiniProfilerStartupModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                if (context.Request.IsLocal)
                {                    
                    MiniProfiler.Start();                    
                }
            };

            context.AuthorizeRequest += (sender, e) =>
            {
                var stopProfiling = false;
                var httpContext = HttpContext.Current;

                if (httpContext == null)
                {
                    stopProfiling = true;
                }
                else
                {
                    // Temporarily removing until we figure out the hammering of request we saw.
                    // var userCanProfile = httpContext.User != null && HttpContext.Current.User.IsInRole(Const.AdminRoleName);
                    var requestIsLocal = httpContext.Request != null && httpContext.Request.IsLocal;

                    // stopProfiling = !userCanProfile && !requestIsLocal
                    stopProfiling = !requestIsLocal;
                }

                if (stopProfiling)
                {
                    MiniProfiler.Stop(true);
                }
            };

            context.EndRequest += (sender, e) =>
            {
                MiniProfiler.Stop();
            };
        }

        public void Dispose()
        {
        }
    }
}
using Microsoft.AspNetCore.SignalR;

using Microsoft.Extensions.Configuration;
using SealWebRTC.Hubs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SealWebRTC.Services
{
    public interface IDatabaseChangeNotificationService
    {
        void config();
    }

//ALTER DATABASE sealwebrtc SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;

//USE[master]
//GO
//SELECT[name], [is_broker_enabled], [service_broker_guid] FROM [sys].[databases]
    public class SqlDependecyService : IDatabaseChangeNotificationService
    {
        private readonly IConfiguration configuration;
        private readonly IHubContext<DashHub> dashHub;

        public SqlDependecyService(IConfiguration configuration, IHubContext<DashHub> dashHub)
        {
            this.configuration = configuration;
            this.dashHub = dashHub;
        }

        public void config()
        {
            SuscribirCambios();
        }

        private void SuscribirCambios()
        {
            try
            {
                string connString = configuration.GetConnectionString("DB");
                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"SELECT Status FROM Meeting", conn))
                    {
                        cmd.Notification = null;
                        SqlDependency dependency = new SqlDependency(cmd);
                        dependency.OnChange += Tabla_Cambio;//new OnChangeEventHandler(Tabla_Cambio); //
                        SqlDependency.Start(connString);
                        cmd.ExecuteReader();
                    }
                }
            }
            catch(Exception ex){

            }
        }

        private void Tabla_Cambio(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                dashHub.Clients.All.SendAsync("UpdateDash");
            }
            SuscribirCambios();
        }
    }
}

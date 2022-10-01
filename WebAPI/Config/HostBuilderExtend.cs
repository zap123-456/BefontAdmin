using Autofac;
using Autofac.Extensions.DependencyInjection;
using Model.Other;
using SqlSugar;

namespace WebAPI.Config
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class HostBuilderExtend
    {
        public static void Register(this WebApplicationBuilder app) {
            app.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            app.Host.ConfigureContainer<ContainerBuilder>(builder =>
            {
                #region 注册sqlsugar
                builder.Register<ISqlSugarClient>(context =>
                {
                    SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
                    {
                        //连接数据库
                        ConnectionString = "server=.;database=authordb;integrated security=true",
                        DbType = DbType.SqlServer,
                        IsAutoCloseConnection = true,
                    });
                    //支持sql语句输出，方便排查问题
                    db.Aop.OnLogExecuted = (sql, par) =>
                    {
                        Console.WriteLine("\r\n");
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}生成Sql语句：{sql}");
                        Console.WriteLine("==============================================================================");
                    };
                    return db;
                });
                #endregion

                # region 注册注册接口和实现层
                builder.RegisterModule(new AutofacModuleRegister());
                #endregion

                # region 注册Automapper映射
                app.Services.AddAutoMapper(typeof(AutoMapperConfigs));
                #endregion
                # region 第一步，注册JWT
                app.Services.Configure<JWTTokenOptions>(app.Configuration.GetSection("JWTTokenOptions"));
                #endregion
            });
        }
    }
}

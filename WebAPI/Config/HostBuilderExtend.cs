using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Model.Other;
using SqlSugar;
using System.Text;

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
            });
            #region 注册Automapper映射
            app.Services.AddAutoMapper(typeof(AutoMapperConfigs));
            #endregion
            #region 第一步，注册JWT
            app.Services.Configure<JWTTokenOptions>(app.Configuration.GetSection("JWTTokenOptions"));
            #endregion
            #region 第二步JWT校验
            //第二步，增加鉴权逻辑
            JWTTokenOptions tokenOptions = new JWTTokenOptions();
            app.Configuration.Bind("JWTTokenOptions", tokenOptions);
            app.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)//Scheme
             .AddJwtBearer(options =>  //这里是配置的鉴权的逻辑
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     //JWT有一些默认的属性，就是给鉴权时就可以筛选了
                     ValidateIssuer = true,//是否验证Issuer
                     ValidateAudience = true,//是否验证Audience
                     ValidateLifetime = true,//是否验证失效时间
                     ValidateIssuerSigningKey = true,//是否验证SecurityKey
                     ValidAudience = tokenOptions.Audience,//
                     ValidIssuer = tokenOptions.Issuer,//Issuer，这两项和前面签发jwt的设置一致
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey))//拿到SecurityKey 
                 };
             });
            #endregion
            //添加跨域策略
            app.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", opt => opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("X-Pagination"));
            });
        }
    }
}

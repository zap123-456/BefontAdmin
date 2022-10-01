using Autofac;
using System.Reflection;

namespace WebAPI.Config
{
    public class AutofacModuleRegister:Autofac.Module
    {
        //注册接口和实现之间的关系(以后的服务和接口对应 直接在下面注册了)
        protected override void Load(ContainerBuilder builder)
        {
            Assembly interfaceAssembly = Assembly.Load("Interface");
            Assembly serviceAssembly = Assembly.Load("Service");
            builder.RegisterAssemblyTypes(interfaceAssembly, serviceAssembly).AsImplementedInterfaces();
        }
    }
}

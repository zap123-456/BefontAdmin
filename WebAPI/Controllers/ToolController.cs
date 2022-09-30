using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Entitys;
using SqlSugar;
using System.Reflection;

namespace WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ToolController : ControllerBase
    {
        //如果一个类的对象是通过DI类创建的，那么这个类的构造函数中声明的所有服务类型参数都会被DI赋值
        public ISqlSugarClient _db;
        public ToolController(ISqlSugarClient db) {
            _db=db;
        }
        [HttpGet]
        public string InitDataBase() {
            string result = "ok";
            //如果不存在就创建数据库
            _db.DbMaintenance.CreateDatabase();
            //创建表
            string naspace = "Model.Entitys";
            Type[] ass=Assembly.LoadFrom(AppContext.BaseDirectory+"Model.dll").GetTypes().Where(p=>p.Namespace== naspace).ToArray();
            _db.CodeFirst.SetStringDefaultLength(200).InitTables(ass);
            //初始化炒鸡管理员和菜单
            Users user = new Users()
            {
                Name = "admin",
                NickName = "炒鸡管理员",
                Password = "123456",
                UserType = 0,
                IsEnable = true,
                Description = "数据库初始化时默认添加的炒鸡管理员",
                CreateDate = DateTime.Now,
                CreateUserId = 0,
                IsDeleted = 0
            };
            long userId = _db.Insertable(user).ExecuteReturnBigIdentity();
            Menu menuRoot = new Menu()
            {
                Name = "菜单管理",
                Index = "menumanager",
                FilePath = "../views/admin/menu/MenuManager",
                ParentId = 0,
                Order = 0,
                IsEnable = true,
                Description = "数据库初始化时默认添加的默认菜单",
                CreateDate = DateTime.Now,
                CreateUserId = userId,
                IsDeleted = 0
            };
            _db.Insertable(menuRoot).ExecuteReturnBigIdentity();
            return result;
        }
    }
}

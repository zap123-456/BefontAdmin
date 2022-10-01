using Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Dto.User;
using Model.Other;
using WebAPI.Config;

namespace WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IUserService _UserService;
        private ICustomJWTService _CustomJWTService;

        public LoginController(IUserService UserService, ICustomJWTService CustomJWTService) { 
            _UserService = UserService;
            _CustomJWTService = CustomJWTService;
        }
        [HttpGet]
        public async Task<ApiResult> GetToken(string name,string password) {
            var res = Task.Run(() => {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password)) {
                    return ResultHelper.Error("参数不能为空");
                }
                UserRes user = _UserService.GetUser(name, password);
                if (string.IsNullOrEmpty(user.Name)) {
                    return ResultHelper.Error("账号密码错误");
                }
                return ResultHelper.Success(_CustomJWTService.GetToken(user));
            });
            return await res;
        }
    }
}

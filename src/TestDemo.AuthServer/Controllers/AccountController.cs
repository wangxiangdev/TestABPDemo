using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace TestDemo.Controllers
{
    [Route("/accounta")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    //[Dependency(ReplaceServices = true)]
    //[ExposeServices(typeof(Volo.Abp.Account.AccountController))]
    public class AccountController : AbpController //Volo.Abp.Account.AccountController
    {
        //public AccountController(IAccountAppService accountAppService) : base(accountAppService)
        //{

        //}

        [Route("login")]
        [HttpGet, HttpPost]
        public IActionResult Login()
        {
            return View();
        }

        [Route("logout")]
        [HttpGet, HttpPost]
        public IActionResult Logout()
        {
            return View();
        }


        //[HttpPost]
        //[Route("register")]
        //public override Task<IdentityUserDto> RegisterAsync(RegisterDto input)
        //{
        //    return AccountAppService.RegisterAsync(input);
        //}

        //[HttpPost]
        //[Route("send-password-reset-code")]
        //public override Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input)
        //{
        //    return AccountAppService.SendPasswordResetCodeAsync(input);
        //}

        //[HttpPost]
        //[Route("verify-password-reset-token")]
        //public Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenInput input)
        //{
        //    return AccountAppService.VerifyPasswordResetTokenAsync(input);
        //}

        //[HttpPost]
        //[Route("reset-password")]
        //public override Task ResetPasswordAsync(ResetPasswordDto input)
        //{
        //    return AccountAppService.ResetPasswordAsync(input);
        //}
    }
}

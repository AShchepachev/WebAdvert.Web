using Amazon.AspNetCore.Identity.Cognito;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;
        private readonly IConfiguration _config;

        public AccountsController(
            SignInManager<CognitoUser> signInManager,
            UserManager<CognitoUser> userManager,
            CognitoUserPool pool,
            IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
            _config = config;
        }


        [HttpGet]
        public async Task<IActionResult> Signup()
        {
            return await Task.Run(() => View());
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("Email", "User with this email already exists");
                    return await Task.Run(() => View());
                }

                user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.Email);

                var createdUser = await _userManager.CreateAsync(user, model.Password);

                if (createdUser.Succeeded)
                {
                    return RedirectToAction("Confirm", new { email = model.Email });
                }
            }

            return await Task.Run(() => View(model));
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(string email)
        {
            ConfirmModel model = new() { Email = email };

            return await Task.Run(() => View(model));
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "The user with given email not found");
                    return await Task.Run(() => View(model));
                }

                var result = await (_userManager as CognitoUserManager<CognitoUser>)?.ConfirmSignUpAsync(user, model.Code, true);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }

                    return await Task.Run(() => View(model));
                }
            }

            return await Task.Run(() => View(model));
        }


        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return await Task.Run(() => View());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                HttpContext.User = await _signInManager.CreateUserPrincipalAsync(_pool.GetUser(model.Email));

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Email", "Email and password do not match pr user does not exist");
                    return await Task.Run(() => View());
                }

            }

            return await Task.Run(() => View(model));
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPassword()
        {
            var user = await _userManager.FindByEmailAsync("artaws@mailinator.com");

            await user.ForgotPasswordAsync();

            return RedirectToPage("ConfirmResetPassword", new ConfirmModel { Email = user.Username });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmResetPassword(ConfirmModel model)
        {
            //if (ModelState.IsValid)
            //{
            //    var user = await _userManager.FindByEmailAsync("artaws@mailinator.com");

            //    if (user == null)
            //    {
            //        ModelState.AddModelError("NotFound", "A user with the given email was not found");
            //        return View(model);
            //    }

            //    await user.ConfirmForgotPasswordAsync(model.ConfirmationCode, model.NewPassword);

            //    if (result.HttpStatusCode == HttpStatusCode.OK)
            //    {
            //        return RedirectToPage("ConfirmResetPassword");
            //    }
            //}

            return await Task.Run(() => View());
        }

        [HttpPost]
        public async Task<IActionResult> Signout()
        {
            if (User.Identity.IsAuthenticated) 
                await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }

        private string GetSecretHash(CognitoUser user)
        {
            var secret = _config["AWS:UserPoolClientSecret"];
            var bytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(bytes);
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(user.Username + user.ClientID)));
        }
    }
}

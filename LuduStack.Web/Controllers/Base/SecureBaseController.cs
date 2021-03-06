﻿using LuduStack.Application;
using LuduStack.Application.Formatters;
using LuduStack.Application.Interfaces;
using LuduStack.Application.Requests.Notification;
using LuduStack.Application.ViewModels;
using LuduStack.Application.ViewModels.User;
using LuduStack.Application.ViewModels.UserPreferences;
using LuduStack.Domain.Core.Attributes;
using LuduStack.Domain.Core.Enums;
using LuduStack.Domain.Core.Extensions;
using LuduStack.Infra.CrossCutting.Abstractions;
using LuduStack.Infra.CrossCutting.Identity.Models;
using LuduStack.Web.Enums;
using LuduStack.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LuduStack.Web.Controllers.Base
{
    public class SecureBaseController : BaseController
    {
        private IServiceProvider services;
        public IServiceProvider Services => services ?? (services = HttpContext?.RequestServices.GetService<IServiceProvider>());

        private IWebHostEnvironment hostEnvironment;
        public IWebHostEnvironment HostEnvironment => hostEnvironment ?? (hostEnvironment = Services.GetService<IWebHostEnvironment>());

        private INotificationSender notificationSender;
        public INotificationSender NotificationSender => notificationSender ?? (notificationSender = Services.GetService<INotificationSender>());

        private UserManager<ApplicationUser> _userManager;
        public UserManager<ApplicationUser> UserManager => _userManager ?? (_userManager = Services.GetService<UserManager<ApplicationUser>>());

        private IImageStorageService _imageStorageService;
        public IImageStorageService ImageStorageService => _imageStorageService ?? (_imageStorageService = Services.GetService<IImageStorageService>());

        private ICookieMgrService _cookieMgrService;
        public ICookieMgrService CookieMgrService => _cookieMgrService ?? (_cookieMgrService = Services.GetService<ICookieMgrService>());

        private IProfileAppService profileAppService;
        public IProfileAppService ProfileAppService => profileAppService ?? (profileAppService = Services.GetService<IProfileAppService>());

        private IUserPreferencesAppService userPreferencesAppService;

        public IUserPreferencesAppService UserPreferencesAppService => userPreferencesAppService ?? (userPreferencesAppService = Services.GetService<IUserPreferencesAppService>());

        public Guid CurrentUserId { get; set; }

        public String CurrentLocale { get; set; }

        public string EnvName { get; private set; }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var notificationClicked = context.HttpContext.Request.Query["notificationclicked"].FirstOrDefault();
            if (notificationClicked != null)
            {
                var notificationId = Guid.Parse(notificationClicked);

                var mediator = Services.GetRequiredService<IMediator>();

                Task.Run(async () => await mediator.Send(new SendNotificationRequest(notificationId)));
            }

            if (User != null && User.Identity.IsAuthenticated)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                CurrentUserId = new Guid(userId);

                ViewBag.CurrentUserId = CurrentUserId;
                ViewBag.ProfileImage = UrlFormatter.ProfileImage(CurrentUserId);

                Claim userIsAdmin = User.FindFirst(x => x.Type == ClaimTypes.Role && x.Value.Equals(Roles.Administrator.ToString()));

                if (userIsAdmin != null)
                {
                    ViewData["user_is_admin"] = "true";
                }

                if (ViewBag.Username == null)
                {
                    string username = User.FindFirstValue(ClaimTypes.Name);

                    SetProfileOnSession(CurrentUserId, username);
                    ViewBag.Username = username ?? Constants.DefaultUsername;
                }

                var msg = context.HttpContext.Request.Query["msg"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    TempData["Message"] = SharedLocalizer[msg];
                }
            }

            CurrentLocale = GetAspNetCultureCookie();
            ViewBag.Locale = CurrentLocale;

            EnvName = string.Format("env-{0}", HostEnvironment.EnvironmentName);

            return base.OnActionExecutionAsync(context, next);
        }

        protected async Task SetProfileOnSession(Guid userId, string userName)
        {
            string sessionUserName = GetSessionValue(SessionValues.Username);

            if (sessionUserName != null && !sessionUserName.Equals(userName))
            {
                SetSessionValue(SessionValues.Username, userName);
            }

            string sessionFullName = GetSessionValue(SessionValues.FullName);

            if (sessionFullName == null)
            {
                ProfileViewModel profile = await ProfileAppService.GetByUserId(userId, ProfileType.Personal);
                if (profile != null)
                {
                    SetSessionValue(SessionValues.FullName, profile.Name);
                }
            }
        }

        protected void SetEmailConfirmed(ApplicationUser user)
        {
            string sessionEmailConfirmed = user.EmailConfirmed.ToString();

            SetEmailConfirmed(sessionEmailConfirmed);
        }

        protected void SetEmailConfirmed()
        {
            string sessionEmailConfirmed = GetSessionValue(SessionValues.EmailConfirmed);
            if (string.IsNullOrWhiteSpace(sessionEmailConfirmed))
            {
                Task<ApplicationUser> task = UserManager.GetUserAsync(User);
                task.Wait();

                ApplicationUser user = task.Result;

                sessionEmailConfirmed = user?.EmailConfirmed.ToString() ?? "False";
            }

            SetEmailConfirmed(sessionEmailConfirmed);
        }

        private void SetEmailConfirmed(string sessionEmailConfirmed)
        {
            SetSessionValue(SessionValues.EmailConfirmed, sessionEmailConfirmed);

            ViewData["emailConfirmed"] = sessionEmailConfirmed;
        }

        protected string GetAvatar()
        {
            return GetCookieValue(SessionValues.UserProfileImageUrl);
        }

        protected void SetAvatar(string profileImageUrl)
        {
            SetCookieValue(SessionValues.UserProfileImageUrl, profileImageUrl, 7, true);
        }

        protected ProfileViewModel SetAuthorDetails(UserGeneratedBaseViewModel vm)
        {
            if (vm == null)
            {
                return null;
            }

            if (vm.Id == Guid.Empty || vm.UserId == Guid.Empty)
            {
                vm.UserId = CurrentUserId;
            }

            ProfileViewModel profile = ProfileAppService.GetUserProfileWithCache(vm.UserId);

            if (profile != null)
            {
                vm.AuthorName = profile.Name;
                vm.AuthorPicture = UrlFormatter.ProfileImage(vm.UserId);
            }

            return profile;
        }

        protected SupportedLanguage SetLanguageFromCulture(string languageCode)
        {
            switch (languageCode)
            {
                case "pt-BR":
                case "pt":
                    return SupportedLanguage.Portuguese;

                case "ru":
                case "ru-RU":
                    return SupportedLanguage.Russian;

                case "de":
                    return SupportedLanguage.German;

                case "es":
                    return SupportedLanguage.Spanish;

                case "bs":
                    return SupportedLanguage.Bosnian;

                case "sr":
                    return SupportedLanguage.Serbian;

                case "hr":
                    return SupportedLanguage.Croatian;

                default:
                    return SupportedLanguage.English;
            }
        }

        protected void SetAspNetCultureCookie(SupportedLanguage language)
        {
            string locale = language.GetAttributeOfType<UiInfoAttribute>()?.Locale;

            locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale;

            SetAspNetCultureCookie(new RequestCulture(locale));
        }

        private void SetAspNetCultureCookie(RequestCulture culture)
        {
            ViewBag.Locale = culture;

            SetCookieValue(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(culture), 365);
        }

        protected string GetAspNetCultureCookie()
        {
            RequestCulture requestLanguage = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture;

            string cookieValue = GetCookieValue(CookieRequestCultureProvider.DefaultCookieName);

            ProviderCultureResult cookie = CookieRequestCultureProvider.ParseCookieValue(cookieValue);

            if (!User.Identity.IsAuthenticated)
            {
                SetAspNetCultureCookie(requestLanguage);

                return requestLanguage.UICulture.Name;
            }
            else
            {
                if (cookie == null || !cookie.Cultures.Any())
                {
                    UserPreferencesViewModel userPrefs = UserPreferencesAppService.GetByUserId(CurrentUserId);

                    if (userPrefs != null && userPrefs.Id != Guid.Empty)
                    {
                        SetAspNetCultureCookie(userPrefs.UiLanguage);
                    }
                    else
                    {
                        SetAspNetCultureCookie(requestLanguage);
                    }

                    return requestLanguage != null ? requestLanguage.UICulture.Name : "en-US";
                }
                else
                {
                    return cookie.Cultures.First().Value;
                }
            }
        }

        #region Upload Management

        #region Main Methods

        private string UploadImage(Guid userId, string imageType, string filename, byte[] fileBytes, params string[] tags)
        {
            Task<string> op = ImageStorageService.StoreImageAsync(userId.ToString(), imageType.ToLower() + "_" + filename, fileBytes, tags);
            op.Wait();

            if (!op.IsCompletedSuccessfully)
            {
                throw op.Exception;
            }

            string url = op.Result;

            return url;
        }

        private string DeleteImage(Guid userId, string filename)
        {
            Task<string> op = ImageStorageService.DeleteImageAsync(userId.ToString(), filename);
            op.Wait();

            if (!op.IsCompletedSuccessfully)
            {
                throw op.Exception;
            }

            string url = op.Result;

            return url;
        }

        private string DeleteImage(Guid userId, string imageType, string filename)
        {
            Task<string> op = ImageStorageService.DeleteImageAsync(userId.ToString(), imageType.ToLower() + "_" + filename);
            op.Wait();

            if (!op.IsCompletedSuccessfully)
            {
                throw op.Exception;
            }

            string url = op.Result;

            return url;
        }

        #endregion Main Methods

        protected string UploadImage(Guid userId, ImageType container, string filename, byte[] fileBytes, params string[] tags)
        {
            string containerName = container.ToString().ToLower();

            return UploadImage(userId, containerName, filename, fileBytes, tags);
        }

        protected string UploadGameImage(Guid userId, ImageType type, string filename, byte[] fileBytes, params string[] tags)
        {
            string result = UploadImage(userId, type.ToString().ToLower(), filename, fileBytes, tags);

            return result;
        }

        protected string UploadContentImage(Guid userId, string filename, byte[] fileBytes, params string[] tags)
        {
            string type = ImageType.ContentImage.ToString().ToLower();
            string result = UploadImage(userId, type, filename, fileBytes, tags);

            return result;
        }

        protected string UploadFeaturedImage(Guid userId, string filename, byte[] fileBytes, params string[] tags)
        {
            string type = ImageType.FeaturedImage.ToString().ToLower();
            string result = UploadImage(userId, type, filename, fileBytes, tags);

            return result;
        }

        protected string DeleteGameImage(Guid userId, ImageType type, string filename)
        {
            string result = DeleteImage(userId, filename);

            return result;
        }

        protected string DeleteFeaturedImage(Guid userId, string filename)
        {
            string result = DeleteImage(userId, filename);

            return result;
        }

        #endregion Upload Management

        #region Cookie Management

        protected string GetCookieValue(SessionValues key)
        {
            string value = CookieMgrService.Get(key.ToString());

            return value;
        }

        private string GetCookieValue(string key)
        {
            string value = CookieMgrService.Get(key);

            return value;
        }

        protected void SetCookieValue(SessionValues key, string value, int? expireTime)
        {
            SetCookieValue(key, value, expireTime, false);
        }

        protected void SetCookieValue(SessionValues key, string value, int? expireTime, bool isEssential)
        {
            CookieMgrService.Set(key.ToString(), value, expireTime, isEssential);
        }

        protected void SetCookieValue(string key, string value, int? expireTime)
        {
            SetCookieValue(key, value, expireTime, false);
        }

        protected void SetCookieValue(string key, string value, int? expireTime, bool isEssential)
        {
            CookieMgrService.Set(key, value, expireTime, isEssential);
        }

        #endregion Cookie Management
    }
}
﻿using LuduStack.Application.Interfaces;
using LuduStack.Application.ViewModels.Giveaway;
using LuduStack.Domain.ValueObjects;
using LuduStack.Web.Areas.Tools.Controllers.Base;
using LuduStack.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuduStack.Web.Areas.Tools.Controllers
{
    public class GiveawayController : ToolsBaseController
    {
        private readonly IGiveawayAppService giveawayAppService;

        public GiveawayController(IGiveawayAppService giveawayAppService)
        {
            this.giveawayAppService = giveawayAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("tools/giveaway/listbyme")]
        public PartialViewResult ListByMe()
        {
            List<GiveawayListItemVo> model;

            OperationResultVo serviceResult = giveawayAppService.GetGiveawaysByMe(CurrentUserId);

            if (serviceResult.Success)
            {
                OperationResultListVo<GiveawayListItemVo> castResult = serviceResult as OperationResultListVo<GiveawayListItemVo>;

                model = castResult.Value.ToList();
            }
            else
            {
                model = new List<GiveawayListItemVo>();
            }

            ViewData["ListDescription"] = SharedLocalizer["My Giveaways"].ToString();

            return PartialView("_ListGiveaways", model);
        }

        [Authorize]
        [Route("tools/giveaway/add/")]
        public IActionResult Add()
        {
            OperationResultVo serviceResult = giveawayAppService.GenerateNew(CurrentUserId);

            if (serviceResult.Success)
            {
                OperationResultVo<GiveawayViewModel> castResult = serviceResult as OperationResultVo<GiveawayViewModel>;

                GiveawayViewModel model = castResult.Value;

                return View("CreateEditWrapper", model);
            }
            else
            {
                return View("CreateEditWrapper", new GiveawayViewModel());
            }
        }

        [Route("tools/giveaway/edit/{id:guid}")]
        public ViewResult Edit(Guid id)
        {
            GiveawayViewModel model;

            OperationResultVo serviceResult = giveawayAppService.GetGiveawayById(CurrentUserId, id);

            OperationResultVo<GiveawayViewModel> castResult = serviceResult as OperationResultVo<GiveawayViewModel>;

            model = castResult.Value;

            model.Description = ContentFormatter.FormatCFormatTextAreaBreaks(model.Description);

            return View("CreateEditWrapper", model);
        }

        [Route("tools/giveaway/save")]
        public JsonResult SaveCourse(GiveawayViewModel vm)
        {
            bool isNew = vm.Id == Guid.Empty;

            try
            {
                vm.UserId = CurrentUserId;

                OperationResultVo<Guid> saveResult = giveawayAppService.SaveGiveaway(CurrentUserId, vm);

                if (saveResult.Success)
                {
                    string url = Url.Action("index", "giveaway", new { area = "tools" });

                    if (isNew)
                    {
                        url = Url.Action("edit", "giveaway", new { area = "tools", id = vm.Id, pointsEarned = saveResult.PointsEarned });

                        NotificationSender.SendTeamNotificationAsync("New Giveaway created!");
                    }

                    return Json(new OperationResultRedirectVo<Guid>(saveResult, url));
                }
                else
                {
                    return Json(new OperationResultVo(false));
                }
            }
            catch (Exception ex)
            {
                return Json(new OperationResultVo(ex.Message));
            }
        }


        [Route("giveaway/{id:guid}")]
        public IActionResult Details(Guid id)
        {
            OperationResultVo result = giveawayAppService.GetGiveawayById(CurrentUserId, id);

            if (result.Success)
            {
                OperationResultVo<GiveawayViewModel> castRestult = result as OperationResultVo<GiveawayViewModel>;

                GiveawayViewModel model = castRestult.Value;

                SetAuthorDetails(model);

                return View("Details", model);
            }
            else
            {
                return null;
            }
        }


        [Route("giveaway/{id:guid}/terms")]
        public IActionResult Terms(Guid id)
        {
            OperationResultVo result = giveawayAppService.GetGiveawayById(CurrentUserId, id);

            if (result.Success)
            {
                OperationResultVo<GiveawayViewModel> castResult = result as OperationResultVo<GiveawayViewModel>;

                var model = new KeyValuePair<Guid, string>(castResult.Value.Id, castResult.Value.TermsAndConditions);

                return View("Terms", model);
            }
            else
            {
                return null;
            }
        }
    }
}
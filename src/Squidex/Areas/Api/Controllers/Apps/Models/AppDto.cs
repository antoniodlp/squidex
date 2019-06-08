﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Areas.Api.Controllers.Assets;
using Squidex.Areas.Api.Controllers.Backups;
using Squidex.Areas.Api.Controllers.Plans;
using Squidex.Areas.Api.Controllers.Rules;
using Squidex.Areas.Api.Controllers.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;
using AllPermissions = Squidex.Shared.Permissions;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppDto : Resource, IGenerateETag
    {
        /// <summary>
        /// The name of the app.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// The version of the app.
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// The id of the app.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The timestamp when the app has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The timestamp when the app has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }

        /// <summary>
        /// The permission level of the user.
        /// </summary>
        public string[] Permissions { get; set; }

        /// <summary>
        /// Gets the current plan name.
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// Gets the next plan name.
        /// </summary>
        public string PlanUpgrade { get; set; }

        public static AppDto FromApp(IAppEntity app, string userId, PermissionSet userPermissions, IAppPlansProvider plans, ApiController controller)
        {
            var permissions = new List<Permission>();

            if (app.Contributors.TryGetValue(userId, out var roleName) && app.Roles.TryGetValue(roleName, out var role))
            {
                permissions.AddRange(role.Permissions);
            }

            if (userPermissions != null)
            {
                permissions.AddRange(userPermissions.ToAppPermissions(app.Name));
            }

            var result = SimpleMapper.Map(app, new AppDto());

            result.Permissions = permissions.ToArray(x => x.Id);
            result.PlanName = plans.GetPlanForApp(app)?.Name;

            if (controller.HasPermission(AllPermissions.AppPlansChange, app.Name))
            {
                result.PlanUpgrade = plans.GetPlanUpgradeForApp(app)?.Name;
            }

            return CreateLinks(result, controller, new PermissionSet(permissions));
        }

        private static AppDto CreateLinks(AppDto result, ApiController controller, PermissionSet permissions)
        {
            var values = new { app = result.Name };

            if (controller.HasPermission(AllPermissions.AppDelete, result.Name, permissions: permissions))
            {
                result.AddDeleteLink("delete", controller.Url<AppsController>(x => nameof(x.DeleteApp), values));
            }

            if (controller.HasPermission(AllPermissions.AppAssetsRead, result.Name, permissions: permissions))
            {
                result.AddGetLink("assets", controller.Url<AssetsController>(x => nameof(x.GetAssets), values));
            }

            if (controller.HasPermission(AllPermissions.AppBackupsRead, result.Name, permissions: permissions))
            {
                result.AddGetLink("backups", controller.Url<BackupsController>(x => nameof(x.GetJobs), values));
            }

            if (controller.HasPermission(AllPermissions.AppClientsRead, result.Name, permissions: permissions))
            {
                result.AddGetLink("clients", controller.Url<AppClientsController>(x => nameof(x.GetClients), values));
            }

            if (controller.HasPermission(AllPermissions.AppContributorsRead, result.Name, permissions: permissions))
            {
                result.AddGetLink("contributors", controller.Url<AppContributorsController>(x => nameof(x.GetContributors), values));
            }

            if (controller.HasPermission(AllPermissions.AppCommon, result.Name, permissions: permissions))
            {
                result.AddGetLink("languages", controller.Url<AppLanguagesController>(x => nameof(x.GetLanguages), values));
            }

            if (controller.HasPermission(AllPermissions.AppCommon, result.Name, permissions: permissions))
            {
                result.AddGetLink("patterns", controller.Url<AppPatternsController>(x => nameof(x.GetPatterns), values));
            }

            if (controller.HasPermission(AllPermissions.AppPlansRead, result.Name, permissions: permissions))
            {
                result.AddGetLink("plans", controller.Url<AppPlansController>(x => nameof(x.GetPlans), values));
            }

            if (controller.HasPermission(AllPermissions.AppRolesRead, result.Name, permissions: permissions))
            {
                result.AddGetLink("roles", controller.Url<AppRolesController>(x => nameof(x.GetRoles), values));
            }

            if (controller.HasPermission(AllPermissions.AppRulesRead, result.Name, permissions: permissions))
            {
                result.AddGetLink("rules", controller.Url<RulesController>(x => nameof(x.GetRules), values));
            }

            if (controller.HasPermission(AllPermissions.AppCommon, result.Name, permissions: permissions))
            {
                result.AddGetLink("schemas", controller.Url<SchemasController>(x => nameof(x.GetSchemas), values));
            }

            if (controller.HasPermission(AllPermissions.AppSchemasCreate, result.Name, permissions: permissions))
            {
                result.AddPostLink("schemas/create", controller.Url<SchemasController>(x => nameof(x.PostSchema), values));
            }

            return result;
        }
    }
}

﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface IContentTextIndex
    {
        Task<List<Guid>?> SearchAsync(string? queryText, IAppEntity app, SearchFilter? filter, SearchScope scope);

        Task ExecuteAsync(NamedId<Guid> appId, NamedId<Guid> schemaId, params IndexCommand[] commands);
    }
}

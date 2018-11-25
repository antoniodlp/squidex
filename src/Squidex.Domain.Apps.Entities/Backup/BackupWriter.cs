﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup.Helpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupWriter : DisposableObjectBase
    {
        private readonly ZipArchive archive;
        private readonly IJsonSerializer serializer;
        private int writtenEvents;
        private int writtenAttachments;

        public int WrittenEvents
        {
            get { return writtenEvents; }
        }

        public int WrittenAttachments
        {
            get { return writtenAttachments; }
        }

        public BackupWriter(IJsonSerializer serializer, Stream stream, bool keepOpen = false)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;

            archive = new ZipArchive(stream, ZipArchiveMode.Create, keepOpen);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                archive.Dispose();
            }
        }

        public Task WriteJsonAsync(string name, object value)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var attachmentEntry = archive.CreateEntry(ArchiveHelper.GetAttachmentPath(name));

            using (var stream = attachmentEntry.Open())
            {
                serializer.Serialize(value, stream);
            }

            writtenAttachments++;

            return TaskHelper.Done;
        }

        public async Task WriteBlobAsync(string name, Func<Stream, Task> handler)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(handler, nameof(handler));

            var attachmentEntry = archive.CreateEntry(ArchiveHelper.GetAttachmentPath(name));

            using (var stream = attachmentEntry.Open())
            {
                await handler(stream);
            }

            writtenAttachments++;
        }

        public void WriteEvent(StoredEvent storedEvent)
        {
            Guard.NotNull(storedEvent, nameof(storedEvent));

            var eventEntry = archive.CreateEntry(ArchiveHelper.GetEventPath(writtenEvents));

            using (var stream = eventEntry.Open())
            {
                serializer.Serialize(storedEvent, stream);
            }

            writtenEvents++;
        }
    }
}

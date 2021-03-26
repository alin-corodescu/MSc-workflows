using System;
using System.IO;
using System.Threading.Tasks;
using Definitions.Adapters;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using NUnit.Framework;
using Workflows.Models.DataEvents;

namespace UnitTests
{
    public class LocalStorageTests
    {
        [Test]
        public async Task PushData_WorksAsExpected()
        {
            var inputPath = $"/home/alin/store/test.txt";

            await LocalFileSystemStorageAdapter.CreateHardLink(inputPath, "/home/alin/store/perm_storage/link.txt");

        }

    }
}
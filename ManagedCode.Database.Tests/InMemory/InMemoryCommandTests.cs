﻿using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Base;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests.InMemory
{
    public class InMemoryCommandTests : BaseCommandTests<int, InMemoryItem>
    {
        private static volatile int _count;
        private InMemoryDataBase _databaseb;

        public InMemoryCommandTests()
        {
            _databaseb = new InMemoryDataBase();
        }

        protected override IDBCollection<int, InMemoryItem> Collection => _databaseb.GetCollection<int, InMemoryItem>();

        protected override int GenerateId()
        {
            _count++;
            return _count;
        }

        public override void Dispose()
        {
            _databaseb.Dispose();
        }
    }
}

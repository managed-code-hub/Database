using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.InMemoryTests;

#if IN_MEMORY || DEBUG
public class InMemoryCollectionTests : BaseCollectionTests<int, InMemoryItem>
{
    public InMemoryCollectionTests() : base(new InMemoryTestContainer())
    {
    }
}
#endif
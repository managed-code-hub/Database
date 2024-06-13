using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.SQLiteTests;

#if SQLITE || DEBUG
public class SQLiteCollectionTests : BaseCollectionTests<int, TestSQLiteItem>
{
    public SQLiteCollectionTests() : base(new SQLiteTestContainer())
    {
    }
}
#endif
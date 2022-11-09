using System;
using ManagedCode.Database.CosmosDB;

namespace ManagedCode.Database.Tests.Common;

public class TestCosmosDbItem : CosmosDBItem, IBaseItem<string>
{
    public string StringData { get; set; }
    public int IntData { get; set; }
    public long LongData { get; set; }
    public float FloatData { get; set; }
    public double DoubleData { get; set; }
    public DateTime DateTimeData { get; set; }
}
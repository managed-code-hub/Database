﻿using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.Core;

public class ServiceCollectionHolder
{
    public ServiceCollectionHolder(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }

    public IServiceCollection ServiceCollection { get; }
}
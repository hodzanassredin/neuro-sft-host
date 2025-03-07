﻿using LlmCommon.Abstractions;
using LlmCommon.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LlmCommon.Implementations
{
    public class Registrator
    {
        public static void Register(IServiceProvider sp) {
            var eventBus = sp.GetRequiredService<IEventBus>();

            var viewHandler = new ViewsEventHandler(sp);
            eventBus.Subscribe(viewHandler);
        }
    }
}

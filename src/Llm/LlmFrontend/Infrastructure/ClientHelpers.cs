using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LlmFrontend.Infrastructure
{
    public static class ClientHelpers
    {
        public static void ScrollToEnd(this IJSRuntime runtime, ElementReference el)
        {
            ((IJSInProcessRuntime)runtime).InvokeVoid("Utils.scrollToEnd", el);
        }
    }
}

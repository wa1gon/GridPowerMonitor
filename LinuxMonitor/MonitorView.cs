// language: csharp
using System;
using Microsoft.Extensions.DependencyInjection; // <- required for GetRequiredService<T>()
// other usings...

namespace LinuxMonitor
{
    // inside code where you have IServiceProvider serviceProvider:
    // var svc = serviceProvider.GetRequiredService<YourServiceType>();
}
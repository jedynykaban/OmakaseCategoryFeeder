using System;

//only for exceptions that are not defined in System namespace and are used in API layer as possible request result
namespace Fusion.Tests.Api.Client.CommLayer
{
    public sealed class UnauthorizedException : Exception { }
}

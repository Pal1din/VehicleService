using Grpc.Core;

namespace VehicleService.API.Extensions;

internal static class GrpcExtensions
{
    internal static Metadata AddTokenToMetadata(this HttpContext context)
    {
        return
        [
            new Metadata.Entry("Authorization", $"Bearer {context.GetToken()}")
        ];
    }
}
using LlmCommon;
using System.Security.Claims;

namespace LlmFrontend.Infrastructure
{
    public static class UserHelper
    {
        public static LlmCommon.Dtos.User ToUser(this ClaimsPrincipal? user)
        {
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {

                //var idStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var idStr = user.FindFirst(ClaimTypes.Name)?.Value;
                var id = idStr == null ? Ids.Empty : Ids.Parse(idStr);
                return new LlmCommon.Dtos.User(
                    id,
                    user.FindFirst(ClaimTypes.Name)?.Value ?? ""
                    //user.FindFirstValue(ClaimTypes.Email) ?? "",
                    //user.Claims
                    //.Where(c => c.Type == ClaimTypes.Role)
                    //.Any(c => c.Value == "Admin")
                );
            }
            else
            {
                return LlmCommon.Dtos.User.Empty;
            }
        }
    }
}

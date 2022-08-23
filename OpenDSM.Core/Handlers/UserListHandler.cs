
using OpenDSM.Core.Models;

namespace OpenDSM.Core.Handlers;

public static class UserListHandler
{
    public static UserModel[] GetUserFromPartials(int maxSize, params string[] partials)
    {
        List<UserModel> users = new();
        int[] u = SQL.Authoriztaion.GetUsersWithPartialUsername(maxSize,partials);
        Parallel.ForEach(u, id =>
        {
            UserModel? user = UserModel.GetByID(id);
            if (user != null)
                users.Add(user);
        });

        return users.ToArray();
    }
}

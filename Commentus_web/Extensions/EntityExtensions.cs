using Commentus_web.Models;
using Microsoft.EntityFrameworkCore;

namespace Commentus_web.Extensions
{
    public static class EntityExtensions
    {
        public static List<TasksSolver>  GetTaskSolvers(this DbSet<TasksSolver> taskSolvers, string userName, Room? room, DateTime timeStamp) =>
            taskSolvers.Include(t => t.User)
                       .Include(t => t.Task)
                       .Where(t => t.User.Name == userName)
                       .Where(t => t.Task.RoomsId == room.Id && t.Task.Timestamp > timeStamp).ToList();
    }
}

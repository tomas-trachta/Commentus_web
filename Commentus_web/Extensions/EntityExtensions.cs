using Commentus_web.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace Commentus_web.Extensions
{
    public static class EntityExtensions
    {
        public static List<TasksSolver>  GetAddedTaskSolvers(this DbSet<TasksSolver> taskSolvers, string userName, Room? room, DateTime timeStamp) =>
            taskSolvers.Include(t => t.User)
                       .Include(t => t.Task)
                       .Where(t => t.User.Name == userName)
                       .Where(t => t.Task.RoomsId == room.Id && t.Task.Timestamp > timeStamp).ToList();

        public static IQueryable<TasksSolver> GetTaskSolvers(this DbSet<TasksSolver> taskSolvers, string userName, DbSet<Room> rooms, string roomName) =>
            taskSolvers.Include(t => t.User).Include(t => t.Task)
                       .Where(t => t.User.Name == userName)
                       .Where(t => t.Task.RoomsId == (rooms.Where(r => r.Name == roomName).First()).Id);

        public static IQueryable<TasksSolver> GetTaskSolversAsAdmin(this DbSet<TasksSolver> taskSolvers, DbSet<Room> rooms, string roomName) =>
            taskSolvers.Include(t => t.Task).Where(t => t.Task.RoomsId ==
                                                         rooms.Where(r => r.Name == roomName).First().Id)
                                                    .OrderBy(t => t.TaskId);

        public static DateTime GetLastMessageTimeStamp(this DbSet<RoomsMessage> roomsMessages, int roomId) =>
            roomsMessages.Where(x => x.RoomId == roomId).OrderBy(x => x.Id).Last().Timestamp;
    }
}

using Microsoft.EntityFrameworkCore.Query;

namespace Commentus_web.Models
{
    public class RoomModel
    {
        public Room? Room { get; set; }
        public IIncludableQueryable<RoomsMember, User>? Members { get; set; }
        public IQueryable<RoomsMessage>? Messages { get; set; }
        public IQueryable<TasksSolver>? Tasks { get; set; }
    }
}

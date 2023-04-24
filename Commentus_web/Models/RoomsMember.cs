using System;
using System.Collections.Generic;

namespace Commentus_web.Models;

public partial class RoomsMember
{
    public int UserId { get; set; }

    public int RoomId { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace Commentus_web.Models;

public partial class RoomsMessage
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int RoomId { get; set; }

    public byte[] Message { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

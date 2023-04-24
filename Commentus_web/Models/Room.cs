using System;
using System.Collections.Generic;

namespace Commentus_web.Models;

public partial class Room
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<RoomsMessage> RoomsMessages { get; set; } = new List<RoomsMessage>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}

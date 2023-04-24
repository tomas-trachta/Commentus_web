using System;
using System.Collections.Generic;

namespace Commentus_web.Models;

public partial class Task
{
    public int Id { get; set; }

    public int RoomsId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime DueDate { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual Room Rooms { get; set; } = null!;

    public virtual ICollection<TasksSolver> TasksSolvers { get; set; } = new List<TasksSolver>();
}

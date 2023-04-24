using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Commentus_web.Models;

public partial class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public byte[] Salt { get; set; } = null!;

    public byte[] Password { get; set; } = null!;

    public bool IsAdmin { get; set; }

    public byte[]? ProfilePicture { get; set; }

    public virtual ICollection<RoomsMessage> RoomsMessages { get; set; } = new List<RoomsMessage>();

    public virtual ICollection<TasksSolver> TasksSolvers { get; set; } = new List<TasksSolver>();
}

namespace Commentus_web.Models
{
    public class TaskModel
    {
        public int RoomsId { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime DueDate { get; set; }

        public List<string> ?Users { get; set; }
    }
}

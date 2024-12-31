using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MunicipalityPortal.Models
{
    // Models for creating and viewing announcements and events
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public List<string> Tags { get; set; }

        public byte[] ThumbnailImage { get; set; }

        public bool IsPastEvent { get; set; }

        public Event()
        {
            Tags = new List<string>();
        }
    }
}

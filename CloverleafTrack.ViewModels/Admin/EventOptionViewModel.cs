using CloverleafTrack.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloverleafTrack.ViewModels.Admin
{

    public class EventOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EventType EventType { get; set; }
        public EventCategory? EventCategory { get; set; }
        public Gender? Gender { get; set; }
        public int AthleteCount { get; set; }

        public string CategoryName => EventCategory?.ToString() ?? "Other";
    }
}

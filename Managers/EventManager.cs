using MunicipalityPortal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MunicipalityPortal.Managers
{
    // Manager for viewing and creating events and announcements

    public class EventManager
    {
        private SortedDictionary<DateTime, List<Event>> eventsByDate;
        private PriorityQueue<Event, DateTime> upcomingEvents;

        // Hash Set for event categories, adapted from:
        // Author: Code Maze
        // Article: HashSet in C#
        // Source: https://code-maze.com/csharp-hashset/

        private Dictionary<string, HashSet<Event>> eventsByCategory;
        private HashSet<string> categories;

        private string filePath = "events.json";

        public EventManager()
        {
            eventsByDate = new SortedDictionary<DateTime, List<Event>>();
            eventsByCategory = new Dictionary<string, HashSet<Event>>();
            upcomingEvents = new PriorityQueue<Event, DateTime>();
            categories = new HashSet<string>();
            LoadEvents();
        }

        // Allow user to post their eventfor the community to view
        public void AddEvent(Event newEvent)
        {
            newEvent.Id = GetNextEventId();

            // Sorted Dictionary for keeping the dates sorted automatically, adapted from:
            // Author: Ankita Saini
            // Article: SortedDictionary Implementation in C#
            // Source: https://www.geeksforgeeks.org/sorteddictionary-implementation-in-c-sharp/

            // Add to eventsByDate
            if (!eventsByDate.ContainsKey(newEvent.Date.Date))
            {
                eventsByDate[newEvent.Date.Date] = new List<Event>();
            }
            eventsByDate[newEvent.Date.Date].Add(newEvent);

            // Dictionary to map event categories (as strings) to sets of events, adapted from:
            // Author: Code Maze
            // Article: Dictionary in C#
            // Source: https://code-maze.com/dictionary-csharp/

            // Add to eventsByCategory
            if (!eventsByCategory.ContainsKey(newEvent.Category))
            {
                eventsByCategory[newEvent.Category] = new HashSet<Event>();
            }
            eventsByCategory[newEvent.Category].Add(newEvent);

            // Priority Queue to keep upcoming events prioritized by their event date, adapted from:
            // Author: Code Maze
            // Article: Priority Queue in C#
            // Source:https://code-maze.com/csharp-priority-queue/

            // Add to upcomingEvents
            upcomingEvents.Enqueue(newEvent, newEvent.Date);

            // Add category
            categories.Add(newEvent.Category);

            SaveEvents();
        }

        // Fetch the current events/announcements stored in the 'events.json' file
        private void LoadEvents()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var events = JsonConvert.DeserializeObject<List<Event>>(json);
                        if (events != null && events.Any())
                        {
                            foreach (var ev in events)
                            {
                                AddEvent(ev);
                            }
                        }
                        else
                        {
                            Console.WriteLine("The events file is empty or contains no valid events.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The events file is empty.");
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing the events file: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while loading events: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No events file found. Starting with an empty event list.");
            }
        }

        // Format an event/announcement as JSON, then save it to the 'events.json' file
        private void SaveEvents()
        {
            var events = GetAllEvents();
            var json = JsonConvert.SerializeObject(events, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // Generate an ID for the event/announcement
        private int GetNextEventId()
        {
            return GetAllEvents().Count > 0 ? GetAllEvents().Max(e => e.Id) + 1 : 1;
        }

        // Add all categories found in the file, to the search page dropdown menu
        public List<Event> GetEventsByCategory(string category)
        {
            if (string.IsNullOrEmpty(category) || category == "All Categories")
            {
                return GetAllEvents();
            }

            return eventsByCategory.TryGetValue(category, out var events)
                ? events.ToList()
                : new List<Event>();
        }

        // Get events based on the dates selected by the user
        public List<Event> GetEventsByDateRange(DateTime startDate, DateTime endDate)
        {
            return eventsByDate
                .Where(kvp => kvp.Key >= startDate.Date && kvp.Key <= endDate.Date)
                .SelectMany(kvp => kvp.Value)
                .OrderBy(e => e.Date)
                .ToList();
        }

        // Add all categories to the list
        public List<string> GetAllCategories()
        {
            return categories.ToList();
        }

        // Get all events based on date
        public List<Event> GetAllEvents()
        {
            return eventsByDate.Values.SelectMany(list => list).OrderBy(e => e.Date).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MunicipalityPortal.Models;

namespace MunicipalityPortal.Managers
{
    public static class IssueManager
    {
        // The use of an ObservableCollection<T> has been adapted from this source:
        // Author:  Bradley Wells
        // Article: ObservableCollection .NET MAUI - UPDATE UI WITH OBSERVABLECOLLECTION IN.NET MAUI
        // Source: https://wellsb.com/csharp/maui/observablecollection-dotnet-maui 

        public static ObservableCollection<Issue> ReportedIssues { get; } = new ObservableCollection<Issue>();


        // Binary Search Tree for issue tracking:
        // Author: Source Code Examples
        // Article: AVL Tree Implementation in C#
        // Source: https://www.sourcecodeexamples.net/2023/10/avl-tree-implementation-in-csharp.html

        private static IssueAVLTree issueTree = new IssueAVLTree();

        // Graphs for establishing relationships between the user submitted service requests
        // Adapted from:
        // Author: interviewer.live
        // Article: Mastering Graph Data Structure in C#
        // Source:https://interviewer.live/c-sharp/mastering-graph-data-structure-csharp/

        private static IssueGraph issueGraph = new IssueGraph();

        public static void AddIssue(Issue issue)
        {
            ReportedIssues.Add(issue);
            issueTree.Insert(issue);
            issueGraph.AddVertex(issue);
            Console.WriteLine($"Issue added. Total issues: {ReportedIssues.Count}");
        }

        public static Issue GetIssueById(string id)
        {
            return issueTree.FindById(id);
        }

        public static List<Issue> GetRelatedIssues(string id, string relationType = null)
        {
            return issueGraph.GetRelatedIssues(id, relationType);
        }

        public static List<Issue> GetPriorityOrderedIssues()
        {
            return issueGraph.GetPriorityOrder();
        }

        public static void UpdateIssueStatus(string id, IssueStatus newStatus, string comment)
        {
            var issue = GetIssueById(id);
            if (issue != null)
            {
                issue.UpdateStatus(newStatus, comment);
            }
        }

        public static string GetDebugInfo()
        {
            return $"Total issues in IssueManager: {ReportedIssues.Count}";
        }
    }
}

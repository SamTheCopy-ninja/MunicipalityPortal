using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MunicipalityPortal.Models
{
    // Class for handling media attachments for each entry
    public class MediaAttachment
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; }

        public MediaAttachment(string fileName, string contentType, long fileSize, string filePath)
        {
            FileName = fileName;
            ContentType = contentType;
            FileSize = fileSize;
            FilePath = filePath;
        }
    }

    // Class/Model for user submitted report info
    public class Issue
    {
        public string Id { get; private set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public MediaAttachment Attachment { get; set; }
        public DateTime ReportedAt { get; set; }
        public IssueStatus Status { get; set; }
        public List<StatusUpdate> StatusHistory { get; private set; }

        public string DisplaySummary => $"{Id} - {ReportedAt:g} - {Category} - {Location} - {Status}";

        public Issue(string location, string category, string description, MediaAttachment attachment)
        {
            Id = GenerateUniqueId();
            Location = location;
            Category = category;
            Description = description;
            Attachment = attachment;
            ReportedAt = DateTime.Now;
            Status = IssueStatus.Pending;
            StatusHistory = new List<StatusUpdate>();
            StatusHistory.Add(new StatusUpdate(Status, "Issue reported", DateTime.Now));
        }

        // Generate an ID for each user submitted service issue
        private string GenerateUniqueId()
        {
            return $"SR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        // Allow users to update the status of an issue
        public void UpdateStatus(IssueStatus newStatus, string comment)
        {
            Status = newStatus;
            StatusHistory.Add(new StatusUpdate(newStatus, comment, DateTime.Now));
        }
    }

    // Status options for the combo box in the "Service Request Status" page
    public enum IssueStatus
    {
        Pending,
        InProgress,
        UnderReview,
        Resolved,
        Closed
    }

    // Class to keep track of status update details
    public class StatusUpdate
    {
        public IssueStatus Status { get; set; }
        public string Comment { get; set; }
        public DateTime Timestamp { get; set; }

        public StatusUpdate(IssueStatus status, string comment, DateTime timestamp)
        {
            Status = status;
            Comment = comment;
            Timestamp = timestamp;
        }
    }

    // Binary Search Tree for issue tracking:
    // Author: Source Code Examples
    // Article: AVL Tree Implementation in C#
    // Source: https://www.sourcecodeexamples.net/2023/10/avl-tree-implementation-in-csharp.html

    public class IssueNode
    {
        public Issue Value { get; set; }
        public IssueNode Left { get; set; }
        public IssueNode Right { get; set; }
        public int Height { get; set; }

        public IssueNode(Issue value)
        {
            Value = value;
            Height = 1;
        }
    }

    // AVL Binary Tree setup
    public class IssueAVLTree
    {
        private IssueNode root;

        public void Insert(Issue issue)
        {
            root = InsertRec(root, issue);
        }

        private IssueNode InsertRec(IssueNode node, Issue issue)
        {
            if (node == null)
                return new IssueNode(issue);

            if (string.Compare(issue.Id, node.Value.Id) < 0)
                node.Left = InsertRec(node.Left, issue);
            else if (string.Compare(issue.Id, node.Value.Id) > 0)
                node.Right = InsertRec(node.Right, issue);
            else
                return node;

            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

            int balance = GetBalance(node);

            // Left Left Case
            if (balance > 1 && string.Compare(issue.Id, node.Left.Value.Id) < 0)
                return RightRotate(node);

            // Right Right Case
            if (balance < -1 && string.Compare(issue.Id, node.Right.Value.Id) > 0)
                return LeftRotate(node);

            // Left Right Case
            if (balance > 1 && string.Compare(issue.Id, node.Left.Value.Id) > 0)
            {
                node.Left = LeftRotate(node.Left);
                return RightRotate(node);
            }

            // Right Left Case
            if (balance < -1 && string.Compare(issue.Id, node.Right.Value.Id) < 0)
            {
                node.Right = RightRotate(node.Right);
                return LeftRotate(node);
            }

            return node;
        }

        private int GetHeight(IssueNode node)
        {
            if (node == null)
                return 0;
            return node.Height;
        }

        private int GetBalance(IssueNode node)
        {
            if (node == null)
                return 0;
            return GetHeight(node.Left) - GetHeight(node.Right);
        }

        private IssueNode RightRotate(IssueNode y)
        {
            IssueNode x = y.Left;
            IssueNode T2 = x.Right;

            x.Right = y;
            y.Left = T2;

            y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;
            x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;

            return x;
        }

        private IssueNode LeftRotate(IssueNode x)
        {
            IssueNode y = x.Right;
            IssueNode T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;
            y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;

            return y;
        }

        // Find an issue in the list, based on the ID entered by the user
        public Issue FindById(string id)
        {
            return FindByIdRec(root, id)?.Value;
        }

        private IssueNode FindByIdRec(IssueNode node, string id)
        {
            if (node == null || node.Value.Id == id)
                return node;

            if (string.Compare(id, node.Value.Id) < 0)
                return FindByIdRec(node.Left, id);

            return FindByIdRec(node.Right, id);
        }
    }

    // Graphs for establishing relationships between the user submitted service requests
    // Adapted from:
    // Author: interviewer.live
    // Article: Mastering Graph Data Structure in C#
    // Source:https://interviewer.live/c-sharp/mastering-graph-data-structure-csharp/

    public class IssueVertex
    {
        public Issue Issue { get; }
        public List<IssueEdge> Edges { get; }
        public bool Visited { get; set; }

        public IssueVertex(Issue issue)
        {
            Issue = issue;
            Edges = new List<IssueEdge>();
            Visited = false;
        }
    }

    // Establish the edges
    public class IssueEdge
    {
        public IssueVertex Source { get; }
        public IssueVertex Target { get; }
        public double Weight { get; }
        public string RelationType { get; } 

        public IssueEdge(IssueVertex source, IssueVertex target, double weight, string relationType)
        {
            Source = source;
            Target = target;
            Weight = weight;
            RelationType = relationType;
        }
    }

    public class IssueGraph
    {
        private Dictionary<string, IssueVertex> vertices;

        public IssueGraph()
        {
            vertices = new Dictionary<string, IssueVertex>();
        }

        // Add the vertex
        public void AddVertex(Issue issue)
        {
            if (!vertices.ContainsKey(issue.Id))
            {
                vertices.Add(issue.Id, new IssueVertex(issue));
                // Create edges with existing vertices based on relationships
                CreateRelationshipEdges(vertices[issue.Id]);
            }
        }

        // Create relationship edges for service requests
        private void CreateRelationshipEdges(IssueVertex newVertex)
        {
            foreach (var vertex in vertices.Values)
            {
                if (vertex != newVertex)
                {
                    // Create edges based on location
                    if (vertex.Issue.Location == newVertex.Issue.Location)
                    {
                        double weight = CalculateLocationBasedWeight(vertex.Issue, newVertex.Issue);
                        AddEdge(newVertex, vertex, weight, "SameLocation");
                    }

                    // Create edges based on category
                    if (vertex.Issue.Category == newVertex.Issue.Category)
                    {
                        double weight = CalculateCategoryBasedWeight(vertex.Issue, newVertex.Issue);
                        AddEdge(newVertex, vertex, weight, "SameCategory");
                    }
                }
            }
        }

        // Weights based on the physical location of the reported issue
        private double CalculateLocationBasedWeight(Issue issue1, Issue issue2)
        {
            TimeSpan timeDifference = issue1.ReportedAt - issue2.ReportedAt;
            return Math.Abs(timeDifference.TotalDays); // Weight based on time difference
        }

        // Weights based on category selected by user while creating their submission
        private double CalculateCategoryBasedWeight(Issue issue1, Issue issue2)
        {
            TimeSpan timeDifference = issue1.ReportedAt - issue2.ReportedAt;
            double statusDifference = Math.Abs((int)issue1.Status - (int)issue2.Status);
            return Math.Abs(timeDifference.TotalDays) + statusDifference;
        }

        private void AddEdge(IssueVertex source, IssueVertex target, double weight, string relationType)
        {
            var edge = new IssueEdge(source, target, weight, relationType);
            source.Edges.Add(edge);
            target.Edges.Add(new IssueEdge(target, source, weight, relationType)); 
        }

        // Get the related issues when a user selects an issue to view
        public List<Issue> GetRelatedIssues(string issueId, string relationType = null)
        {
            if (!vertices.ContainsKey(issueId))
                return new List<Issue>();

            var relatedIssues = new List<Issue>();
            var queue = new Queue<IssueVertex>();
            var visited = new HashSet<string>();

            queue.Enqueue(vertices[issueId]);
            visited.Add(issueId);

            while (queue.Count > 0)
            {
                var currentVertex = queue.Dequeue();

                foreach (var edge in currentVertex.Edges)
                {
                    var neighborVertex = edge.Source == currentVertex ? edge.Target : edge.Source;

                    if (!visited.Contains(neighborVertex.Issue.Id) &&
                        (relationType == null || edge.RelationType == relationType))
                    {
                        visited.Add(neighborVertex.Issue.Id);
                        relatedIssues.Add(neighborVertex.Issue);
                        queue.Enqueue(neighborVertex);
                    }
                }
            }

            return relatedIssues;
        }

        // Priority Queue based on the issue importance, adapted from 
        // Author: Code Maze
        // Article: Priority Queue in C#
        // Source:https://code-maze.com/csharp-priority-queue/

        public List<Issue> GetPriorityOrder()
        {
            var priorityQueue = new PriorityQueue<Issue, double>();
            var result = new List<Issue>();

            foreach (var vertex in vertices.Values)
            {
                double priority = CalculateIssuePriority(vertex);
                priorityQueue.Enqueue(vertex.Issue, priority);
            }

            while (priorityQueue.Count > 0)
            {
                result.Add(priorityQueue.Dequeue());
            }

            return result;
        }

        // Calculate the priority of an issue
        private double CalculateIssuePriority(IssueVertex vertex)
        {
            
            double priority = 0;

            // Factor 1: Status weight
            priority += (int)vertex.Issue.Status * 10;

            // Factor 2: Age of issue 
            priority += (DateTime.Now - vertex.Issue.ReportedAt).TotalDays;

            // Factor 3: Number of related issues 
            priority -= vertex.Edges.Count * 5;

            return priority;
        }
    }
}

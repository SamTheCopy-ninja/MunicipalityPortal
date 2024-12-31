# Greenville Municipality Portal

The **Greenville Municipality Portal** is a user-friendly WPF application designed to help citizens report issues, keep track of local events, and monitor service request statuses. This platform streamlines communication between the municipality and the community, ensuring efficient service delivery.  
### In this new update, users can now stay up to date with the status of any reported service request, update the status, or view related issues  


## :minidisc: Demo video of the updated build:  [New Features Video](https://youtu.be/SdF8b1O9Mck)  

# Implementation Report: Data Structure Explanation

### 1. **Overview of "Service Request Status" Feature**

The "Service Request Status" and "Report Issues" feature allow users to submit, track, and update reports on community issues. Key functionalities include:
- Submitting service requests with associated metadata (e.g., location, category, description).
- Attaching media files to service requests.
- Viewing status updates on service requests.
- Accessing related issues and priority-ordered lists based on urgency or relevance.

To handle and integrate the service requests feature for this part of the POE, the Municipality portal app has been expanded to implement several data structures, each handling different aspects of performance, flexibility, and accuracy in managing service requests.

### 2. **Data Structures and Their Roles**

#### 2.1 **AVL Tree for Efficient Issue Retrieval**
An AVL Tree structure is used in the application to maintain an ordered list of submitted issues, allowing the app to quickly perform insertion and search operations. By balancing the tree after each insertion, the AVL Tree minimizes search time to O(log n), making it highly efficient when users need to locate specific issues by ID.

**Key Components:**
- **Class:** `IssueAVLTree`
- **Node Class:** `IssueNode` represents individual tree nodes containing `Issue` objects.
- **Methods:** `Insert` for adding new issues, `FindById` for locating issues.

**Code Example: Issue Insertion and Retrieval**
```csharp
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

public class IssueAVLTree
{
    private IssueNode root;

    public void Insert(Issue issue)
    {
        root = InsertRec(root, issue);
    }

    public Issue FindById(string id)
    {
        return FindByIdRec(root, id)?.Value;
    }
}
```

The AVL Tree improves the retrieval performance by maintaining a balanced structure, allowing quick searches for issues when users query by ID.

#### 2.2 **Graph for Related Issue Management**
The `IssueGraph` data structure represents relationships between issues based on similarities in location or category. By connecting issues with edges that describe these relationships, the application can identify and retrieve related issues efficiently, providing additional context when users view a specific issue.

**Key Components:**
- **Classes:** `IssueVertex` and `IssueEdge`
- **Graph Structure:** `IssueGraph` with list representation, for easy traversal.
- **Methods:** `AddVertex` for adding a new issue, `CreateRelationshipEdges` for connecting similar issues, and `GetRelatedIssues` for finding connected issues.

**Code Example: Adding Related Issues**
```csharp
public void AddVertex(Issue issue)
{
    if (!vertices.ContainsKey(issue.Id))
    {
        vertices.Add(issue.Id, new IssueVertex(issue));
        CreateRelationshipEdges(vertices[issue.Id]);
    }
}

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
```

By using a graph structure, the app can quickly identify issues that are similar or geographically related, allowing users to make connections between related reports.

#### 2.3 **Priority Queue for Issue Sorting**
The priority queue structure is used in the `IssueGraph` class to create a priority-ordered list of issues. This ordering is based on attributes such as the issue’s status, the age of the issue, and the number of related issues, giving users an efficient way to view and address urgent matters.

**Key Components:**
- **Priority Queue Library:** Utilizes `PriorityQueue<Issue, double>` in C#.
- **Method:** `GetPriorityOrder` to retrieve a list of issues sorted by their calculated priority.

**Code Example: Priority Calculation and Retrieval**
```csharp
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
```

Using a priority queue structure improves the system's efficiency by surfacing the most critical issues based on pre-defined urgency criteria, improving user experience in issue management.

#### 2.4 **ObservableCollection for Real-Time UI Updates**
The `ObservableCollection<Issue>` in the `IssueManager` class manages all reported issues. By using `ObservableCollection`, we can automatically update the user interface whenever there is a change in the issue list, providing real-time responsiveness to user actions.

**Code Example: Managing Reported Issues Collection**
```csharp
public static ObservableCollection<Issue> ReportedIssues { get; } = new ObservableCollection<Issue>();

public static void AddIssue(Issue issue)
{
    ReportedIssues.Add(issue);
    issueTree.Insert(issue);
    issueGraph.AddVertex(issue);
    Console.WriteLine($"Issue added. Total issues: {ReportedIssues.Count}");
}
```

The ObservableCollection not only handles storage for the user-reported issues but also allows dynamic updates to the UI, enhancing the application's interactivity.

---

### 3. **Conclusion**

By implementing these specific data structures, the portal app can optimize key functions in the "Service Request Status" feature, delivering enhanced performance and user experience:
- **AVL Tree** for fast and efficient issue lookup.
- **Graph Structure** for identifying relationships between issues.
- **Priority Queue** for dynamic sorting based on urgency.
- **ObservableCollection** for real-time updates to the UI.

This structure allows for a responsive and scalable application, ensuring that users can easily track, manage, and resolve service requests in the Municipality Portal.

### 4. Performance Analysis

### Space Complexity
```
??? AVL Tree: O(n) for n issues
??? Graph Structure: O(V + E) where V is vertices and E is edges
??? Priority Queue: O(n) for n issues
??? Observable Collection: O(n) for n issues
```

### Time Complexity for Key Operations
```
??? Issue Lookup (AVL Tree): O(log n)
??? Related Issue Discovery (Graph): O(V + E)
??? Priority-based Retrieval: O(log n)
??? Status Updates: O(1)
```

#### Additional Notes:
- `n` represents the total number of issues in the system
- `V` represents the number of vertices (issues) in the graph
- `E` represents the number of edges (relationships) between issues
- Status updates are constant time O(1) as they only modify property values 

## Installation and Setup  :bulb:

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/SamTheCopy-ninja/MunicipalityPortal.git
2. Open the Project:  
Open the solution in Visual Studio.

3. Build and Run:  
Build the project using Visual Studio.  
Run the application on your local machine.

## Features  :bulb:

### 1. **Report Issues**
   - Allows users to submit detailed reports on various community issues such as:
     - **Garbage Collection Issues**
     - **Streetlight Outages**
     - **Water Leaks**
     - And many more...
   - Users can attach media (images or PDFs) to provide additional context.
   - Includes a progress bar to track report completion, with dynamic messages to guide the user through the process.
   - The report is stored along with any attached media, making it easy for municipality officials to review and address issues.
     
### 2. **View Reports**
   - Users can view all reported issues, including details such as:
     - **Location**
     - **Category**
     - **Description**
     - **Time of report**
   - Attached media (images or PDFs) can be previewed directly in the app.
     
### 3. **Post an Event or Announcement**
   - Users can now post an event or announcement, for fellow community members to view, based on the following:
     - **Title of the event or announcement**
     - **Description for the event or announcement**
     - **Category**
     - **Date for the event or announcement**
     - **Thumbnail image if needed**
   - Attached thumbnail images are previewed directly in the app.

### 4. **View Local Events or Announcement**
   - Users can also view local events and announcements posted to the community portal:
     - **Each event is displayed in a card**
     - **Past events are indicated to the user**
     - **Events are sorted based on the date**
     - **Users can search and filter the results based on dates, categories, or search terms**
     - **Other events and announcements are recommended based on the user's searches**
    
### 5. **Monitor service request status**
   - Users can also monitor the status of a reported service issue:
     - **Users can view a report containing details about the service request**
     - **Users can update the status of a service request, and include a comment message**
     - **User can filter service requests based on the ID**
     - **Users can view related service requests (based on category or location)**
     - **Users can also view any media attachments for service requests**

## Technologies Used  :wrench:

- **C# and XAML** (WPF) [for building the application interface and functionality](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/?view=netdesktop-8.0). 

- **Microsoft.Win32 and OpenDialog** [for handling file attachments](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/windows/how-to-open-common-system-dialog-box?view=netdesktop-8.0).

## Media Attachments  :floppy_disk:

Users can attach the following file types when reporting an issue:
- **Images**: `.jpg`, `.png`
- **PDFs**: `.pdf`

File size is limited to **20MB**.

## How to Use  :bulb:

1. **Reporting Issues**:
   - Click on the "Report Issues" button from the main menu.
   - Enter the issue details: location, category, and description.
   - Attach media if necessary.
   - Click **Submit** to report the issue.
   
2. **Viewing Reports**:
   - Navigate to the "View Reports" section.
   - Select any report to view its details.
   - Click on **View Media** to see the attached images or PDFs.

3. **View Local Events and Announcements**:
   - Click on the "View Local Events and Announcements" button from the main menu.
   - View the current events/announcements posted by other users.
   - Click on **Post an Event/Announcement** to add your own event.

4. **Post an Event or Announcement**:
   - Fill in the form with your event details such as a title and description.
   - Add a thumbnail image to your post if required.

5. **View service request status**:
   - Click on the "Service Request Status" button.
   - Click on a reported issue in the list.
   - View the report of the issue in the collapsable display.
   - Update the status of the issue if required.
   - View any media attachments, if included.
   - Discover related issues, if present.


## Project Creator  :computer:

`Samkelo Tshabalala - from the 13th Coffin Software Development Group`

## Code Attribution  :bookmark_tabs:

- **Binary Search Tree for Issue Tracking**  
  **Author**: Source Code Examples  
  **Article**: [AVL Tree Implementation in C#](https://www.sourcecodeexamples.net/2023/10/avl-tree-implementation-in-csharp.html)  
  **Source**: Source Code Examples  

- **Graphs for Establishing Relationships Between the User Submitted Service Requests**  
  Adapted from:  
  **Author**: interviewer.live  
  **Article**: [Mastering Graph Data Structure in C#](https://interviewer.live/c-sharp/mastering-graph-data-structure-csharp/)  
  **Source**: interviewer.live 

- **ViewModel**  
  Based on the tutorial:  
  **Author**: Raj Kumar  
  **Article**: [Simple MVVM Pattern in WPF](https://www.c-sharpcorner.com/UploadFile/raj1979/simple-mvvm-pattern-in-wpf/)  
  **Source**: C# Corner

- **Relay Command Interface**  
  Based on the tutorial:  
  **Author**: Rikam Palkar  
  **Article**: [ICommand Interface In MVVM](https://www.c-sharpcorner.com/article/icommand-interface-in-mvvm/)  
  **Source**: C# Corner

- **OpenFileDialog**  
  Functionality adapted from:  
  **Author**: CSharp Corner  
  **Article**: [OpenFileDialog In WPF](https://www.c-sharpcorner.com/uploadfile/mahesh/openfiledialog-in-wpf/)  
  **Source**: C# Corner

- **SortedDictionary for Auto-sorted Dates**  
  Adapted from:  
  **Author**: Ankita Saini  
  **Article**: [SortedDictionary Implementation in C#](https://www.geeksforgeeks.org/sorteddictionary-implementation-in-c-sharp/)  
  **Source**: GeeksforGeeks

- **Dictionary to map event categories**  
  Adapted from:  
  **Author**: Code Maze  
  **Article**: [Dictionary in C#](https://code-maze.com/dictionary-csharp/)  
  **Source**: Code Maze

- **Priority Queue for Upcoming Events**  
  Adapted from:  
  **Author**: Code Maze  
  **Article**: [Priority Queue in C#](https://code-maze.com/csharp-priority-queue/)  
  **Source**: Code Maze
using C4Sharp.Diagrams.Builders;
using C4Sharp.Elements;
using C4Sharp.Elements.Relationships;

namespace DrawC4Diagram.Diagrams;

public class CreateTaskSequenceSample : SequenceDiagram
{
    protected override string Title => "Create new Task Sequence diagram";

    protected override IEnumerable<Structure> Structures => new Structure[]
    {
    new Container("A1", "Web Application", ContainerType.None, "JavaScript and Blazor",
        "Enter task name abd submit"),
    new Container("A2", "Web Application", ContainerType.None, "JavaScript and Blazor",
        "Call API"),

    Bound("b", "Web Api",
        new("bA", "TaskListApi", ComponentType.None, "Minimal Api",
            "CreateTaskList"),
        new("bB", "Service Layer", ComponentType.None, "Class Library",
            "CreateTaskList"),
        new("bC", "Service layer", ComponentType.None, "Class Library",
            "CheckMaxTaskListPerUser: Check how many assigned to user"),
        new("bD", "Infrasture layer", ComponentType.None, "Class Library",
            "Repository function")
    ),

    new Container("A4", "Database", ContainerType.Database, "SQL",
        "Save into db")
    };

    protected override IEnumerable<Relationship> Relationships => new[]
    {
        It("A1") > It("A2") | ("Enter new task name", "SaveTaskListModel()"),
        It("A2") > It("bA") | ("Api Post", "CreateTaskList()"),
        It("bA") > It("bB") | ("ITaskListService", "CreateTaskList()"),
        It("bB") > It("bC") | ("ITaskListService ", "CheckMaxTaskListPerUser()"),
        It("bC") > It("bD") | ("ITaskListRepository", "AddAsync()"),
        It("bD") > It("A4") | ("Save")
    };
}

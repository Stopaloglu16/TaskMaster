using C4Sharp.Diagrams;
using C4Sharp.Diagrams.Builders;
using C4Sharp.Elements.Containers;
using C4Sharp.Elements.Relationships;
using static DrawC4Diagram.Structures.System;
using static DrawC4Diagram.Structures.User;

namespace DrawC4Diagram.Diagrams;

public class CreateTaskContainerSample : ContainerDiagram
{
    protected override string Title => "TaskMaster create a new task";
    protected override DiagramLayout FlowVisualization => DiagramLayout.LeftRight;

    protected override IEnumerable<C4Sharp.Elements.Structure> Structures => new C4Sharp.Elements.Structure[]
    {
        AdminUser,
        TaskMasterWeb,
        TaskMasterApi,
        new Database("TmDb", "Sql Db", "TaskMaster", ""),
    };


    protected override IEnumerable<Relationship> Relationships => new Relationship[]
    {
        AdminUser > TaskMasterWeb | "Enter task name",
        TaskMasterWeb >= TaskMasterApi | ("Call create task", "Json/HTTPS"),
        TaskMasterApi > this["TmDb"] | "Save the task",
    };
}

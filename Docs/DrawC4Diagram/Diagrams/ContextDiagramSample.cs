using C4Sharp.Diagrams;
using C4Sharp.Diagrams.Builders;
using C4Sharp.Elements;
using C4Sharp.Elements.Relationships;
using static DrawC4Diagram.Structures.System;
using static DrawC4Diagram.Structures.User;

namespace DrawC4Diagram.Diagrams;

public class ContextDiagramSample : ContextDiagram
{

    protected override string Title => "Component diagram for TaskMaster";

    protected override DiagramLayout FlowVisualization => DiagramLayout.TopDown;

    protected override IEnumerable<Structure> Structures => new Structure[]
    {
          AdminUser,
          TaskUser,
          ReadOnlyUser,
          TaskMasterWeb
    };

    protected override IEnumerable<Relationship> Relationships => new[]
    {
        AdminUser > TaskMasterWeb,
        TaskUser > TaskMasterWeb,
        ReadOnlyUser > TaskMasterWeb
    };
}

// See https://aka.ms/new-console-template for more information
using C4Sharp.Diagrams;
using C4Sharp.Diagrams.Plantuml;
using C4Sharp.Diagrams.Themes;
using DrawC4Diagram.Diagrams;

Console.WriteLine("Star to draw");


string workingDirectory = Environment.CurrentDirectory;

// This will get the current PROJECT directory
string projectDirectory1 = Directory.GetParent(workingDirectory)!.Parent!.Parent!.FullName;


var diagrams = new DiagramBuilder[]
{
    new ContextDiagramSample(),
    new CreateTaskContainerSample(),
    new CreateTaskSequenceSample()
};

var path = Path.Combine(projectDirectory1, "Images");

new PlantumlContext()
    .UseDiagramImageBuilder()
    //.UseDiagramSvgImageBuilder()
    .Export(path, diagrams, new ParadisoTheme());
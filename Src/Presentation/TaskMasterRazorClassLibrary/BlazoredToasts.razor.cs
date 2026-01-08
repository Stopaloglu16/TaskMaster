using Microsoft.AspNetCore.Components;
using TaskMasterRazorClassLibrary.Config;
using TaskMasterRazorClassLibrary.Services;

namespace TaskMasterRazorClassLibrary;

public partial class BlazoredToasts
{

    [Inject]
    private IToastService toastService { get; set; }


    [Parameter]
    public string SuccessClass { get; set; }

    [Parameter]
    public string SuccessIconClass { get; set; } = "fas fa-check";


    [Parameter]
    public string InfoClass { get; set; }

    [Parameter]
    public string InfoIconClass { get; set; } = "fas fa-info";


    [Parameter]
    public string WarningClass { get; set; }

    [Parameter]
    public string WarningIconClass { get; set; } = "fas fa-exclamation";


    [Parameter]
    public string ErrorClass { get; set; }

    [Parameter]
    public string ErrorIconClass { get; set; } = "fas fa-bug";




    [Parameter]
    public ToastPosition toastposition { get; set; } = ToastPosition.TopRight;

    [Parameter]
    public int Timeout { get; set; } = 5;


    private string PositionClass { get; set; } = string.Empty;



    internal List<ToastInstance> ToastList { get; set; } = new List<ToastInstance>();

    protected override void OnInitialized()
    {
        toastService.OnShow += ShowToast;
        PositionClass = $"position-{toastposition.ToString().ToLower()}";
    }

    public void RemoveToast(Guid toastId)
    {
        InvokeAsync(() =>
        {
            var toastInstance = ToastList.SingleOrDefault(x => x.Id == toastId);
            ToastList.Remove(toastInstance);
            StateHasChanged();
        });

    }

    private ToastSettings BuildToastSettings(ToastLevel level, string message, string header)
    {
        switch (level)
        {
            case ToastLevel.Success:
                return new ToastSettings(string.IsNullOrWhiteSpace(header) ? "Success" : header, message, "bg-success", SuccessClass, SuccessIconClass);
            case ToastLevel.Info:
                return new ToastSettings(string.IsNullOrWhiteSpace(header) ? "Info" : header, message, "bg-info", InfoClass, InfoIconClass);
            case ToastLevel.Warning:
                return new ToastSettings(string.IsNullOrWhiteSpace(header) ? "Warning" : header, message, "bg-warning", WarningClass, WarningIconClass);
            case ToastLevel.Error:
                return new ToastSettings(string.IsNullOrWhiteSpace(header) ? "Error" : header, message, "bg-danger", ErrorClass, ErrorIconClass);

        }

        throw new InvalidOperationException();


    }

    private void ShowToast(ToastLevel level, string message, string header)
    {
        InvokeAsync(() =>
        {
            var settings = BuildToastSettings(level, message, header);

            var toast = new ToastInstance
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.Now,
                toastSettings = settings
            };

            ToastList.Add(toast);

            var timeOut = Timeout * 1000;

            var toastTimer = new System.Timers.Timer(timeOut);
            toastTimer.Elapsed += (sender, args) => { RemoveToast(toast.Id); };
            toastTimer.AutoReset = false;
            toastTimer.Start();

            StateHasChanged();

        });
    }

}

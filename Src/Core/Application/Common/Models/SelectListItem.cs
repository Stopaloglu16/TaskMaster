namespace Application.Common.Models;
public class SelectListItem
{
    //
    // Summary:
    //     Gets or sets a value that indicates whether this System.Web.Mvc.SelectListItem
    //     is disabled.
    public bool Disabled { get; set; }

    //
    // Summary:
    //     Gets or sets a value that indicates whether this System.Web.Mvc.SelectListItem
    //     is selected.
    //
    // Returns:
    //     true if the item is selected; otherwise, false.
    public bool Selected { get; set; }

    //
    // Summary:
    //     Gets or sets the text of the selected item.
    //
    // Returns:
    //     The text.
    public string Text { get; set; }

    //
    // Summary:
    //     Gets or sets the value of the selected item.
    //
    // Returns:
    //     The value.
    public int Value { get; set; }

    //
    // Summary:
    //     Initializes a new instance of the System.Web.Mvc.SelectListItem class.
    public SelectListItem()
    {
    }
}

using Microsoft.AspNetCore.Components;

namespace Fiszki.Components.Pages.Generate;

public partial class LoadingOverlay
{
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
}

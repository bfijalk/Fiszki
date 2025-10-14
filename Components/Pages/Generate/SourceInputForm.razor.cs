using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fiszki.Components.Pages.Generate;

public partial class SourceInputForm
{
    [Parameter] public SourceInputModel Value { get; set; } = new();
    [Parameter] public EventCallback<SourceInputModel> OnChange { get; set; }
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public bool IsGenerating { get; set; }
    
    private MudForm _form = null!;
    private SourceInputModel _model = new();
    
    protected override void OnParametersSet()
    {
        _model = Value with { };
    }
    
    private async Task HandleSubmit()
    {
        await _form.Validate();
        if (_form.IsValid)
        {
            await OnSubmit.InvokeAsync();
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender && !Value.Equals(_model))
        {
            await OnChange.InvokeAsync(_model);
        }
    }
}

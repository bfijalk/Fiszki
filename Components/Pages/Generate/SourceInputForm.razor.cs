using Microsoft.AspNetCore.Components;

namespace Fiszki.Components.Pages.Generate;

public partial class SourceInputForm
{
    [Parameter] public SourceInputModel Value { get; set; } = new();
    [Parameter] public EventCallback<SourceInputModel> OnChange { get; set; }
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public bool IsGenerating { get; set; }
    
    private SourceInputModel _model = new();
    
    protected override void OnParametersSet()
    {
        _model = Value with { };
    }
    
    private async Task HandleSubmit()
    {
        if (string.IsNullOrWhiteSpace(_model.Text) || _model.Text.Length < 50) return;
        await OnSubmit.InvokeAsync();
    }
    
    private async Task HandleTextInput(ChangeEventArgs e)
    {
        var text = e.Value?.ToString() ?? string.Empty;
        _model = _model with { Text = text };
        await OnChange.InvokeAsync(_model);
    }
    
    private async Task HandleLanguageChange(ChangeEventArgs e)
    {
        var lang = e.Value?.ToString() ?? _model.Language;
        _model = _model with { Language = lang };
        await OnChange.InvokeAsync(_model);
    }
    
    private async Task HandleMaxCardsChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            if (value < 1) value = 1;
            if (value > 50) value = 50;
            _model = _model with { MaxCards = value };
            await OnChange.InvokeAsync(_model);
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

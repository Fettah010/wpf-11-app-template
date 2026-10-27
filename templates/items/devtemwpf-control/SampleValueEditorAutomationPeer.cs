using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace DevTemWpf.App.Controls;

/// <summary>Automation peer for SampleValueEditor.</summary>
public class SampleValueEditorAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
{
    private readonly SampleValueEditor _owner;

    public SampleValueEditorAutomationPeer(SampleValueEditor owner) : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore() => nameof(SampleValueEditor);

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Spinner;

    protected override string GetNameCore()
    {
        return string.IsNullOrEmpty(_owner.Title) ? nameof(SampleValueEditor) : _owner.Title;
    }

    public override object GetPattern(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.Value)
        {
            return this;
        }
        return base.GetPattern(patternInterface);
    }

    // IValueProvider
    public string Value => _owner.Value.ToString();
    public bool IsReadOnly => false;
    public void SetValue(string value)
    {
        if (int.TryParse(value, out var parsed))
        {
            _owner.Value = Math.Clamp(parsed, _owner.Minimum, _owner.Maximum);
        }
    }
}

using System.Windows;
using ICSharpCode.AvalonEdit;

namespace Kesmai.WorldForge.UI.Controls;

/// <summary>
/// A <see cref="TextEditor"/> that exposes the <see cref="Text"/> property as a
/// dependency property so it can participate in WPF bindings.
/// </summary>
public class BindableTextEditor : TextEditor
{
    private bool _isUpdating;

    /// <summary>
    /// Identifies the <see cref="Text"/> dependency property.
    /// </summary>
    public new static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(BindableTextEditor),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged));

    /// <summary>
    /// Gets or sets the editor's text content.
    /// </summary>
    public new string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public BindableTextEditor()
    {
        TextChanged += (_, __) =>
        {
            if (_isUpdating)
                return;

            try
            {
                _isUpdating = true;
                SetCurrentValue(TextProperty, base.Text);
            }
            finally
            {
                _isUpdating = false;
            }
        };
    }

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (BindableTextEditor)d;
        if (editor._isUpdating)
            return;

        try
        {
            editor._isUpdating = true;
            ((TextEditor)editor).Text = e.NewValue as string ?? string.Empty;
        }
        finally
        {
            editor._isUpdating = false;
        }
    }
}

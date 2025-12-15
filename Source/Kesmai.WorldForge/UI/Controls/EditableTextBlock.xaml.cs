using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Kesmai.WorldForge.UI.Controls;

public partial class EditableTextBlock : UserControl
{
    private string _oldText;
    
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), 
        typeof(string), typeof(EditableTextBlock), new PropertyMetadata(String.Empty, OnTextChanged));

    public bool IsEditable
    {
        get => (bool)GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }
    
    public static readonly DependencyProperty IsEditableProperty =
        DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(true));

    public bool IsInEditMode
    {
        get 
        {
            if (IsEditable)
                return (bool)GetValue(IsInEditModeProperty);
            else
                return false;
        }
        set
        {
            if (IsEditable)
            {
                if (value) 
                    _oldText = Text;
                
                SetValue(IsInEditModeProperty, value);
            }
        }
    }
    
    public static readonly DependencyProperty IsInEditModeProperty =
        DependencyProperty.Register(nameof(IsInEditMode), typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false));

    public string TextFormat
    {
        get => (string)GetValue(TextFormatProperty);
        set => SetValue(TextFormatProperty, value);
    }
    
    public static readonly DependencyProperty TextFormatProperty =
        DependencyProperty.Register(nameof(TextFormat), typeof(string), typeof(EditableTextBlock),
            new PropertyMetadata("{0}", OnTextFormatChanged, CoerceTextFormat));

    private static readonly DependencyPropertyKey FormattedTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FormattedText), typeof(string), typeof(EditableTextBlock),
            new PropertyMetadata(String.Empty));

    public static readonly DependencyProperty FormattedTextProperty = FormattedTextPropertyKey.DependencyProperty;

    public string FormattedText
    {
        get => (string)GetValue(FormattedTextProperty);
    }

    public event EventHandler TextChanged;

    public EditableTextBlock()
	{
		InitializeComponent();
        
        base.Focusable = true;
        base.FocusVisualStyle = null;

        UpdateFormattedText();
	}
    
    void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        textBox.Focus();
        textBox.SelectAll();
    }

    void TextBox_LostFocus(object sender, RoutedEventArgs args)
        => IsInEditMode = false;
    
    void TextBox_KeyDown(object sender, KeyEventArgs args)
    {
        if (args.Key is not (Key.Enter or Key.Escape))
            return;

        IsInEditMode = false;
        
        if (args.Key is Key.Escape)
            Text = _oldText;
        
        if (TextChanged != null)
            TextChanged.Invoke(this, EventArgs.Empty);
        
        args.Handled = true;
    }

    private static void OnTextChanged(DependencyObject textBox, DependencyPropertyChangedEventArgs e)
    {
        if (textBox is not EditableTextBlock editableTextBlock)
            return;
        
        editableTextBlock.UpdateFormattedText();
    }

    private static void OnTextFormatChanged(DependencyObject textBox, DependencyPropertyChangedEventArgs e)
    {
        if (textBox is not EditableTextBlock editableTextBlock)
            return;
        
        editableTextBlock.UpdateFormattedText();
    }

    private static object CoerceTextFormat(DependencyObject d, object baseValue)
    {
        return String.IsNullOrEmpty(baseValue as string) ? "{0}" : baseValue;
    }

    private void UpdateFormattedText()
    {
        SetValue(FormattedTextPropertyKey, String.Format(TextFormat, Text));
    }
}
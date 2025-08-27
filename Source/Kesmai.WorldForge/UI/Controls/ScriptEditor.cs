using System;
using System.Collections.Generic;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Kesmai.WorldForge.UI.Controls;

/// <summary>
/// Text editor that displays a read-only method signature and braces while allowing
/// the user to edit only the script body.
/// </summary>
public class ScriptEditor : TextEditor
{
    private bool _isUpdating;
    private int _prefixLength;
    private int _suffixLength;

    /// <summary>
    /// Identifies the <see cref="Body"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(
            nameof(Body),
            typeof(string),
            typeof(ScriptEditor),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBodyChanged));

    /// <summary>
    /// Identifies the <see cref="Header"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(ScriptEditor),
            new FrameworkPropertyMetadata(string.Empty, OnHeaderChanged));

    /// <summary>
    /// Gets or sets the editable body of the script.
    /// </summary>
    public string Body
    {
        get => (string)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>
    /// Gets or sets the read-only method signature displayed above the body.
    /// </summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public ScriptEditor()
    {
        TextChanged += (_, __) =>
        {
            if (_isUpdating)
                return;

            try
            {
                _isUpdating = true;
                var text = base.Text ?? string.Empty;
                if (text.Length >= _prefixLength + _suffixLength)
                {
                    var body = text.Substring(_prefixLength, text.Length - _prefixLength - _suffixLength);
                    SetCurrentValue(BodyProperty, body);
                }
            }
            finally
            {
                _isUpdating = false;
            }
        };
    }

    private static void OnBodyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (ScriptEditor)d;
        editor.UpdateDocument();
    }

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (ScriptEditor)d;
        editor.UpdateDocument();
    }

    private void UpdateDocument()
    {
        if (_isUpdating)
            return;

        try
        {
            _isUpdating = true;

            var nl = Environment.NewLine;
            var header = Header ?? string.Empty;
            var body = Body ?? string.Empty;
            var prefix = header + nl + "{" + nl;
            var suffix = nl + "}";

            _prefixLength = prefix.Length;
            _suffixLength = suffix.Length;

            base.Text = prefix + body + suffix;
            TextArea.Caret.Offset = _prefixLength;
            TextArea.ReadOnlySectionProvider = new HeaderFooterReadOnlySectionProvider(this);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private class HeaderFooterReadOnlySectionProvider : IReadOnlySectionProvider
    {
        private readonly ScriptEditor _editor;

        public HeaderFooterReadOnlySectionProvider(ScriptEditor editor)
        {
            _editor = editor;
        }

        public bool CanInsert(int offset)
        {
            return offset >= _editor._prefixLength &&
                   offset <= _editor.Document.TextLength - _editor._suffixLength;
        }

        public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
        {
            int start = Math.Max(segment.Offset, _editor._prefixLength);
            int end = Math.Min(segment.EndOffset, _editor.Document.TextLength - _editor._suffixLength);
            if (start < end)
                yield return new TextSegment { StartOffset = start, Length = end - start };
        }
    }
}

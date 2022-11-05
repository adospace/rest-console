using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using RestConsole.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestConsole.Editor;

internal class CommandMargin : AbstractMargin
{
    private readonly DocumentEditorViewModel _document;
    private readonly CommandRunner _commandRunner;
    private Dictionary<int, CommandViewModel> _commands = new();

    public CommandMargin(DocumentEditorViewModel document, CommandRunner commandRunner)
    {
        _document = document;
        _commandRunner = commandRunner;
        _document.PropertyChanged += Document_PropertyChanged;
    }

    private void Document_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _commands = _document.Commands.ToDictionary(_ => _.Command.StartLineIndex + 1, _ => _);
        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(24.0, 0.0);
    }

    public override void Render(DrawingContext drawingContext)
    {
        var textView = TextView;
        var renderSize = Bounds.Size;

        if (textView is { VisualLinesValid: true })
        {
            var foreground = GetValue(TextBlock.ForegroundProperty);
            foreach (var line in textView.VisualLines)
            {
                var lineNumber = line.FirstDocumentLine.LineNumber;
                if (_commands.TryGetValue(lineNumber, out var _))
                {
                    var pixelSize = PixelSnapHelpers.GetPixelSize(this);

                    var y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
                    var fill = new SolidColorBrush(Colors.Red);
                    var rect = new Rect(
                        pixelSize.Width / 2,
                        y + pixelSize.Height / 2 - textView.VerticalOffset,
                        Bounds.Width - pixelSize.Width,
                        line.Height - pixelSize.Height);

                    drawingContext.DrawRectangle(fill, null, rect);
                }
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.Handled && TextView != null && TextArea != null)
        {
            e.Handled = true;
            var renderSize = Bounds.Size;
            var pixelSize = PixelSnapHelpers.GetPixelSize(this);

            var pos = e.GetPosition(TextView);
            pos = new Point(pixelSize.Width / 2, pos.Y.CoerceValue(pixelSize.Width / 2, TextView.Bounds.Height) + TextView.VerticalOffset);
            var vl = TextView.GetVisualLineFromVisualTop(pos.Y);
            if (vl == null)
                return;
            var lineNumber = vl.FirstDocumentLine.LineNumber;
            if (_commands.TryGetValue(lineNumber, out var command))
            {
                var y = vl.GetTextLineVisualYPosition(vl.TextLines[0], VisualYPosition.TextTop);
                var rect = new Rect(
                    pixelSize.Width / 2,
                    y + pixelSize.Height / 2 - TextView.VerticalOffset,
                    Bounds.Width - pixelSize.Width,
                    vl.Height - pixelSize.Height);

                if (rect.Contains(pos))
                {
                    _commandRunner.Execute(command);
                }
            }

            //TextArea.Focus();

            //var currentSeg = GetTextLineSegment(e);
            //if (currentSeg == SimpleSegment.Invalid)
            //    return;
            //TextArea.Caret.Offset = currentSeg.Offset + currentSeg.Length;
            //e.Pointer.Capture(this);
            //if (e.Pointer.Captured == this)
            //{
            //    _selecting = true;
            //    _selectionStart = new AnchorSegment(Document, currentSeg.Offset, currentSeg.Length);
            //    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            //    {
            //        if (TextArea.Selection is SimpleSelection simpleSelection)
            //            _selectionStart = new AnchorSegment(Document, simpleSelection.SurroundingSegment);
            //    }
            //    TextArea.Selection = Selection.Create(TextArea, _selectionStart);
            //    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            //    {
            //        ExtendSelection(currentSeg);
            //    }
            //    TextArea.Caret.BringCaretToView(0);
            //}
        }
    }
}

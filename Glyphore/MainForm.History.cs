using System.Text.Json;

namespace Glyphore;

internal sealed partial class MainForm
{
    private sealed record HistoryEntry(GlyphoreScene Scene, string Signature);

    private readonly List<HistoryEntry> _history = [];
    private int _historyIndex = -1;
    private bool _restoringHistory;
    private DateTime _lastHistoryWriteUtc = DateTime.MinValue;
    private readonly Button _undoButton = new GlyphButton();
    private readonly Button _redoButton = new GlyphButton();

    private void InitializeHistory()
    {
        _history.Clear();
        _historyIndex = -1;
        _lastHistoryWriteUtc = DateTime.MinValue;
        RecordHistory(force: true);
        // The first user edit must create a second entry instead of replacing the initial state.
        _lastHistoryWriteUtc = DateTime.MinValue;
        UpdateHistoryButtons();
    }

    private static string HistorySignature(GlyphoreScene scene)
        => JsonSerializer.Serialize(scene);

    private void RecordHistory(bool force = false)
    {
        if (_restoringHistory || !_sceneReady) return;

        var clone = _scene.Clone();
        string signature = HistorySignature(clone);
        if (_historyIndex >= 0 && _history[_historyIndex].Signature == signature)
        {
            UpdateHistoryButtons();
            return;
        }

        if (_historyIndex < _history.Count - 1)
            _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);

        DateTime now = DateTime.UtcNow;
        bool coalesce = !force && _historyIndex > 0 && now - _lastHistoryWriteUtc < TimeSpan.FromMilliseconds(320);
        if (coalesce)
        {
            _history[_historyIndex] = new HistoryEntry(clone, signature);
        }
        else
        {
            _history.Add(new HistoryEntry(clone, signature));
            _historyIndex = _history.Count - 1;
        }

        const int maxEntries = 160;
        if (_history.Count > maxEntries)
        {
            int remove = _history.Count - maxEntries;
            _history.RemoveRange(0, remove);
            _historyIndex = Math.Max(0, _historyIndex - remove);
        }

        _lastHistoryWriteUtc = now;
        UpdateHistoryButtons();
    }

    private void UndoScene()
    {
        if (_historyIndex <= 0) return;
        RestoreHistoryIndex(_historyIndex - 1);
    }

    private void RedoScene()
    {
        if (_historyIndex < 0 || _historyIndex >= _history.Count - 1) return;
        RestoreHistoryIndex(_historyIndex + 1);
    }

    private void RestoreHistoryIndex(int index)
    {
        if (index < 0 || index >= _history.Count) return;

        ExitImportedMode();
        _restoringHistory = true;
        try
        {
            _historyIndex = index;
            _scene = _history[index].Scene.Clone();
            _activeMaskId = null;
            _preview.ActiveMaskId = null;
            UpdateDiscordPresenceContext();
            _sceneReady = true;
            LoadActiveLayerIntoEditor(refreshCommonControls: true);
            SyncGlobalTransformControls();
            SyncPostProcessControls();
            _preview.RestartAnimation();
            _status.Text = Localization.English
                ? $"History {_historyIndex + 1}/{_history.Count}"
                : $"Historial {_historyIndex + 1}/{_history.Count}";
        }
        finally
        {
            _restoringHistory = false;
            _lastHistoryWriteUtc = DateTime.MinValue;
            UpdateHistoryButtons();
        }
    }

    private void UpdateHistoryButtons()
    {
        _undoButton.Enabled = _historyIndex > 0;
        _redoButton.Enabled = _historyIndex >= 0 && _historyIndex < _history.Count - 1;
    }
}

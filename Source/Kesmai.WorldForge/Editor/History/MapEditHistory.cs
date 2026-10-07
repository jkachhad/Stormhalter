using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Kesmai.WorldForge.Editor;

/// <summary>
/// Sent after an edit is undone or redone, so views showing the affected tiles can refresh.
/// </summary>
public record MapEditApplied(MapEdit Edit);

/// <summary>
/// Undo and redo stacks for edits to region tiles.
/// </summary>
public sealed class MapEditHistory : ObservableObject
{
	/// <summary>
	/// The number of edits kept for undo; the oldest is dropped beyond this.
	/// </summary>
	private const int Capacity = 200;

	private readonly List<MapEdit> _undo = new List<MapEdit>();
	private readonly List<MapEdit> _redo = new List<MapEdit>();

	public bool CanUndo => _undo.Count > 0;
	public bool CanRedo => _redo.Count > 0;

	/// <summary>
	/// Gets the description of the edit that would be undone next, or null.
	/// </summary>
	public string UndoDescription => CanUndo ? _undo[^1].Description : null;

	/// <summary>
	/// Gets the description of the edit that would be redone next, or null.
	/// </summary>
	public string RedoDescription => CanRedo ? _redo[^1].Description : null;

	/// <summary>
	/// Raised when an edit is recorded, undone or redone.
	/// </summary>
	public event EventHandler Changed;

	/// <summary>
	/// Starts recording an edit. Capture each tile before changing it, then commit.
	/// </summary>
	public MapEdit Begin(string description)
	{
		return new MapEdit(this, description);
	}

	/// <summary>
	/// Records a change to a single tile whose previous contents were captured earlier.
	/// </summary>
	public void Record(string description, SegmentRegion region, int x, int y, TileSnapshot before)
	{
		var edit = Begin(description);

		edit.Capture(region, x, y, before);
		edit.Commit();
	}

	public bool Undo(Segment segment)
	{
		if (segment is null || !CanUndo)
			return false;

		var edit = _undo[^1];

		_undo.RemoveAt(_undo.Count - 1);
		_redo.Add(edit);

		edit.Apply(segment, redo: false);

		OnChanged();
		WeakReferenceMessenger.Default.Send(new MapEditApplied(edit));

		return true;
	}

	public bool Redo(Segment segment)
	{
		if (segment is null || !CanRedo)
			return false;

		var edit = _redo[^1];

		_redo.RemoveAt(_redo.Count - 1);
		_undo.Add(edit);

		edit.Apply(segment, redo: true);

		OnChanged();
		WeakReferenceMessenger.Default.Send(new MapEditApplied(edit));

		return true;
	}

	/// <summary>
	/// Forgets all edits, e.g. when another segment is opened.
	/// </summary>
	public void Clear()
	{
		if (_undo.Count is 0 && _redo.Count is 0)
			return;

		_undo.Clear();
		_redo.Clear();

		OnChanged();
	}

	internal void Push(MapEdit edit)
	{
		_undo.Add(edit);

		if (_undo.Count > Capacity)
			_undo.RemoveAt(0);

		// a new edit replaces whatever could have been redone.
		_redo.Clear();

		OnChanged();
	}

	private void OnChanged()
	{
		OnPropertyChanged(nameof(CanUndo));
		OnPropertyChanged(nameof(CanRedo));
		OnPropertyChanged(nameof(UndoDescription));
		OnPropertyChanged(nameof(RedoDescription));

		Changed?.Invoke(this, EventArgs.Empty);
	}
}

/// <summary>
/// One undoable edit: the before and after contents of every tile it changed.
/// </summary>
public sealed class MapEdit
{
	private readonly MapEditHistory _history;

	private readonly Dictionary<(SegmentRegion Region, int X, int Y), Entry> _entries
		= new Dictionary<(SegmentRegion Region, int X, int Y), Entry>();

	private bool _committed;

	public string Description { get; }

	/// <summary>
	/// Gets the regions with tiles changed by this edit.
	/// </summary>
	public IEnumerable<SegmentRegion> Regions => _entries.Keys.Select(key => key.Region).Distinct();

	internal MapEdit(MapEditHistory history, string description)
	{
		_history = history;

		Description = description;
	}

	/// <summary>
	/// Captures the tile's current contents as its state before this edit. Call before changing the tile;
	/// tiles already captured by this edit are ignored.
	/// </summary>
	public void Capture(SegmentRegion region, int x, int y)
	{
		Capture(region, x, y, null);
	}

	/// <summary>
	/// Uses a snapshot taken earlier as the tile's state before this edit.
	/// </summary>
	public void Capture(SegmentRegion region, int x, int y, TileSnapshot before)
	{
		if (_committed || region is null)
			return;

		var key = (region, x, y);

		if (_entries.ContainsKey(key))
			return;

		_entries[key] = new Entry(before ?? TileSnapshot.Capture(region.GetTile(x, y)));
	}

	/// <summary>
	/// Captures the tiles' contents after the edit and records it, unless nothing actually changed.
	/// </summary>
	/// <returns>True if the edit was recorded.</returns>
	public bool Commit()
	{
		if (_committed)
			return false;

		_committed = true;

		foreach (var ((region, x, y), entry) in _entries.ToArray())
		{
			entry.After = TileSnapshot.Capture(region.GetTile(x, y));

			if (entry.Before.IsEquivalent(entry.After))
				_entries.Remove((region, x, y));
		}

		if (_entries.Count is 0)
			return false;

		_history.Push(this);
		return true;
	}

	/// <summary>
	/// Determines whether this edit changed the tile at the location.
	/// </summary>
	public bool Affects(SegmentRegion region, int x, int y)
	{
		return _entries.ContainsKey((region, x, y));
	}

	internal void Apply(Segment segment, bool redo)
	{
		foreach (var ((region, x, y), entry) in _entries)
		{
			// the region may have been deleted since the edit was made.
			if (!segment.Regions.Contains(region))
				continue;

			var snapshot = redo ? entry.After : entry.Before;

			snapshot.Restore(region, x, y);
		}
	}

	private sealed class Entry
	{
		public TileSnapshot Before { get; }
		public TileSnapshot After { get; set; }

		public Entry(TileSnapshot before)
		{
			Before = before;
		}
	}
}

using System;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public sealed class SegmentProject : IDisposable
{
    private FileSystemWatcher _watcher;
    private Segment _activeSegment;

    public SegmentProject()
    {
        WeakReferenceMessenger.Default.Register<ActiveSegmentChanged>(this, (_, message) =>
        {
            var segment = message.Value;

            Pause();
            Reset();

            _activeSegment = segment;

            WatchSegment();
        });

        WeakReferenceMessenger.Default.Register<SegmentChanged>(this, (_, message) =>
        {
            if (_activeSegment is null || !ReferenceEquals(_activeSegment, message.segment))
                return;

            Pause();
            Reset();

            WatchSegment();
        });

        WeakReferenceMessenger.Default.Register<SegmentSerialize>(this, (_, message) => Pause());
        WeakReferenceMessenger.Default.Register<SegmentSerialized>(this, (_, message) => Unpause());
    }

    public void Start(string path)
    {
        _watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
        };

        _watcher.Changed += OnChanged;
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        
        _watcher.EnableRaisingEvents = true;
    }

    public void Pause()
    {
        if (_watcher != null)
            _watcher.EnableRaisingEvents = false;
    }
    
    public void Unpause()
    {
        if (_watcher != null)
            _watcher.EnableRaisingEvents = true;
    }

    public void Reset()
    {
        if (_watcher is null)
            return;
        
        _watcher.Changed -= OnChanged;
        _watcher.Created -= OnCreated;
        _watcher.Deleted -= OnDeleted;
        _watcher.Renamed -= OnRenamed;

        _watcher = null;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (!IsIgnoredPath(e.FullPath))
            WeakReferenceMessenger.Default.SendDelayed(new SegmentFileChangedMessage(e), TimeSpan.FromSeconds(1.0));
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!IsIgnoredPath(e.FullPath))
            WeakReferenceMessenger.Default.SendDelayed(new SegmentFileCreatedMessage(e), TimeSpan.FromSeconds(1.0));
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (!IsIgnoredPath(e.FullPath))
            WeakReferenceMessenger.Default.SendDelayed(new SegmentFileDeletedMessage(e), TimeSpan.FromSeconds(1.0));
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (!IsIgnoredPath(e.FullPath))
            WeakReferenceMessenger.Default.SendDelayed(new SegmentFileRenamedMessage(e), TimeSpan.FromSeconds(1.0));
    }

    private void WatchSegment()
    {
        if (_activeSegment is null)
            return;

        var path = _activeSegment.Directory;

        if (String.IsNullOrWhiteSpace(path))
            return;

        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);

        Start(path);
    }
    
    private bool IsIgnoredPath(string path) => 
        path.Contains(@"\bin\") || path.Contains(@"\obj\") || path.EndsWith("~");

    public void Dispose() => _watcher.Dispose();
}

public class SegmentFileChangedMessage(FileSystemEventArgs value) : ValueChangedMessage<FileSystemEventArgs>(value);
public class SegmentFileCreatedMessage(FileSystemEventArgs value) : ValueChangedMessage<FileSystemEventArgs>(value);
public class SegmentFileDeletedMessage(FileSystemEventArgs value) : ValueChangedMessage<FileSystemEventArgs>(value);
public class SegmentFileRenamedMessage(RenamedEventArgs value) : ValueChangedMessage<RenamedEventArgs>(value);

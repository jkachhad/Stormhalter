using System;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public sealed class SegmentProject : IDisposable
{
    private FileSystemWatcher _watcher;

    public SegmentProject()
    {
        WeakReferenceMessenger.Default.Register<SegmentChangedMessage>(this, (_, message) =>
        {
            var segment = message.Value;

            Pause();
            Reset();

            if (segment != null)
            {
                var path = segment.Path;
                
                if (!Directory.Exists(path))
                    throw new DirectoryNotFoundException(path);
                
                Start(segment.Path);
            }
        });
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
            WeakReferenceMessenger.Default.Send(new SegmentFileChangedMessage(e));
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!IsIgnoredPath(e.FullPath))
            WeakReferenceMessenger.Default.Send(new SegmentFileCreatedMessage(e));
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (!IsIgnoredPath(e.FullPath))
            WeakReferenceMessenger.Default.Send(new SegmentFileDeletedMessage(e));
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (!IsIgnoredPath(e.FullPath))
            WeakReferenceMessenger.Default.Send(new SegmentFileRenamedMessage(e));
    }
    
    private bool IsIgnoredPath(string path) => 
        path.Contains(@"\bin\") || path.Contains(@"\obj\") || path.EndsWith("~");

    public void Dispose() => _watcher.Dispose();
}

public class SegmentFileChangedMessage(FileSystemEventArgs value) : ValueChangedMessage<FileSystemEventArgs>(value);
public class SegmentFileCreatedMessage(FileSystemEventArgs value) : ValueChangedMessage<FileSystemEventArgs>(value);
public class SegmentFileDeletedMessage(FileSystemEventArgs value) : ValueChangedMessage<FileSystemEventArgs>(value);
public class SegmentFileRenamedMessage(RenamedEventArgs value) : ValueChangedMessage<RenamedEventArgs>(value);
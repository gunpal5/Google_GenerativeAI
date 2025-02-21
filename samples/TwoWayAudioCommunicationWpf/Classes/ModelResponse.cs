using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TwoWayAudioCommunicationWpf.Classes;

public class ModelResponse:INotifyPropertyChanged
{
    public ModelResponse(string text)
    {
        Text = text;
    }
    string _text;
    private bool _isSpeaking;
    private bool _isInterrupted;
    private bool _isFinished;
    private string? _file;
    private bool _isConnected;
    private bool _isDisconnected;

    public string Text
    {
        get
        {
            return _text;
        }
        set
        {
            _text = value;
            OnPropertyChanged("Text");
        }
    }

    public bool IsSpeaking
    {
        get => _isSpeaking;
        set
        {
            if (value == _isSpeaking) return;
            _isSpeaking = value;
            OnPropertyChanged(nameof(IsSpeaking));
        }
    }

    public bool IsInterrupted
    {
        get => _isInterrupted;
        set
        {
            if (value == _isInterrupted) return;
            _isInterrupted = value;
            OnPropertyChanged(nameof(IsInterrupted));
        }
    }

    public bool IsFinished
    {
        get => _isFinished;
        set
        {
            if (value == _isFinished) return;
            _isFinished = value;
            OnPropertyChanged();
        }
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (value == _isConnected) return;
            _isConnected = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisconnected
    {
        get => _isDisconnected;
        set
        {
            if (value == _isDisconnected) return;
            _isDisconnected = value;
            OnPropertyChanged();
        }
    }

    public string? File
    {
        get => _file;
        set
        {
            if (value == _file) return;
            _file = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using GenerativeAI;
using GenerativeAI.Live;
using GenerativeAI.Types;
using TwoWayAudioCommunicationWpf.AudioHelper;
using TwoWayAudioCommunicationWpf.Classes;
using TwoWayAudioCommunicationWpf.Constants;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace TwoWayAudioCommunicationWpf;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// Provides functionality for a multimodal live chat application using Generative AI.
/// </summary>
public partial class MainWindow : FluentWindow, INotifyPropertyChanged
{
    private MultiModalLiveClient? _multiModalLiveClient;
    private ModelResponse? _currentResponse;
    private readonly NAudioHelper _audioHelper = new();

    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            if (value == _isRecording) return;
            _isRecording = value;
            OnPropertyChanged("IsRecording");
        }
    }

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRecording = false;

    /// <summary>
    /// Gets or sets the collection of model responses displayed in the UI.
    /// </summary>
    public ObservableCollection<ModelResponse> ModelResponses { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of available audio input devices.
    /// </summary>
    public List<string> AvailableDevices { get; set; } = new();


    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        InitializeAudioDevices();
        DataContext = this; // Set DataContext for bindings

        _audioHelper.AudioDataReceived += AudioDataReceivedEventHandler;
    }

    /// <summary>
    /// Initializes the list of available audio input devices.
    /// </summary>
    private void InitializeAudioDevices()
    {
        AvailableDevices = NAudioHelper.GetAvailableMicrophones().ToList();
        selectedDevice.ItemsSource = AvailableDevices; // Bind directly
    }

    /// <summary>
    /// Event handler for the Start/Stop Chat button click.
    /// </summary>
    private async void BtnStartChat_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_multiModalLiveClient == null)
            {
                await InitializeChat();
            }
            else
            {
                await StopChat();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK,
                MessageBoxImage.Error); // Generic error message.  Consider more specific handling.
        }
    }

    private async void PlayAudioButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ModelResponse response } &&
            !string.IsNullOrEmpty(response.File) && File.Exists(response.File))
        {
            try
            {
                //Disable the button
                if (sender is UIElement uiElement)
                {
                    uiElement.IsEnabled = false;
                }

                await _audioHelper.PlayAudioAsync(response.File);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing audio: " + ex.Message);
            }
            finally
            {
                //Enable the button
                if (sender is UIElement uiElement)
                {
                    uiElement.IsEnabled = true;
                }
            }
        }
    }

    /// <summary>
    /// Initializes the chat session, sets up the client, and starts recording.
    /// </summary>
    private async Task InitializeChat()
    {
        try
        {
            btnStartChat.IsEnabled = false;
            btnStartChat.Content = AppConstants.ConnectingText;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var device = (string?)selectedDevice.SelectedItem;
            if (string.IsNullOrEmpty(device))
                throw new ArgumentException("Please select an audio device."); // More specific exception

            if (string.IsNullOrEmpty(EnvironmentVariables.GOOGLE_API_KEY))
                throw new InvalidOperationException(
                    "Please set the GOOGLE_API_KEY environment variable."); //More specific exception.

            var platform =
                new GoogleAIPlatformAdapter(EnvironmentVariables.GOOGLE_API_KEY); // Consider dependency injection
            
            // var serviceAccountJson = Environment.GetEnvironmentVariable("Google_Service_Account_Json");
            // var authenticator = new GoogleServiceAccountAuthenticator(serviceAccountJson);
            // var platform = new VertextPlatformAdapter(authenticator:authenticator);
            var config = new GenerationConfig() { ResponseModalities = new List<Modality> { Modality.AUDIO } };

            _multiModalLiveClient =
                new MultiModalLiveClient(platform, "gemini-2.0-flash-exp", config); // Consider dependency injection
            _multiModalLiveClient.UseGoogleSearch = true;


            RegisterClientEvents();

            await _multiModalLiveClient.ConnectAsync(cancellationToken);
            StartRecording(device); //No need to pass cancellation token, as it's handled within StartRecording
            btnStartChat.Content = AppConstants.StopChatText;
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully (e.g., update UI)
            MessageBox.Show($"Chat initialization was cancelled.", "Information", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show($"Invalid input: {ex.Message}", "Input Error", MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex) // Catch-all for unexpected errors.
        {
            MessageBox.Show(
                "Failed to initialize chat. Please check your settings and try again.\nDetails: " + ex.Message,
                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnStartChat.IsEnabled = true;
            _cancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Registers event handlers for the MultiModalLiveClient events.
    /// </summary>
    private void RegisterClientEvents()
    {
        if (_multiModalLiveClient == null) return;

        _multiModalLiveClient.MessageReceived += (sender, args) =>
        {
            if (args.Payload.SetupComplete != null)
            {
                Dispatcher.Invoke(() =>
                {
                    ModelResponses.Insert(0,
                        new ModelResponse("Connected. Now you can start chatting.") { IsConnected = true });
                });
            }
        };

        _multiModalLiveClient.AudioChunkReceived += (sender, args) =>
        {
            Dispatcher.Invoke(() => // Batch UI updates for better performance
            {
                if (_currentResponse == null)
                {
                    _currentResponse = new ModelResponse("Audio received") { IsSpeaking = true, Text = "Speaking..." };
                    ModelResponses.Insert(0, _currentResponse);
                }
                else
                {
                    _currentResponse.IsSpeaking = true;
                    //_currentResponse.Text = "Speaking..."; // Consider not updating the text every chunk.
                }

                _audioHelper.BufferWavePlay(args.Buffer, args.HeaderInfo.SampleRate, args.HeaderInfo.Channels,
                    args.HeaderInfo.BitsPerSample);
            });
        };

        _multiModalLiveClient.AudioReceiveCompleted += (sender, payload) =>
        {
            _audioHelper.ClearPlayback(); //Consider if this needs to be inside the Dispatcher.Invoke
            var tmpFile = Path.GetTempFileName() + ".wav";

            //Consider handling IOException
            try
            {
                File.WriteAllBytes(tmpFile,
                    GenerativeAI.Live.Helper.AudioHelper.AddWaveHeader(payload.Buffer, payload.HeaderInfo.Channels,
                        payload.HeaderInfo.SampleRate, payload.HeaderInfo.BitsPerSample));
            }
            catch (IOException ex)
            {
                Dispatcher.Invoke(() =>
                    MessageBox.Show("Failed to write audio to temporary file: " + ex.Message, "File Error"));
                return; // Exit the handler if file writing fails.
            }

            Dispatcher.Invoke(() =>
            {
                if (_currentResponse != null)
                {
                    _currentResponse.IsSpeaking = false;
                    _currentResponse.Text = "Finished";
                    _currentResponse.IsFinished = true;
                    _currentResponse.File = tmpFile;
                    _currentResponse = null;
                }
            });
        };

        _multiModalLiveClient.GenerationInterrupted += (sender, payload) =>
        {
            _audioHelper.StopPlayback(); //Consider if this needs to be inside the Dispatcher.Invoke
            Dispatcher.Invoke(() =>
            {
                if (_currentResponse != null)
                {
                    _currentResponse.IsSpeaking = false;
                    _currentResponse.Text = "Interrupted";
                    _currentResponse.IsInterrupted = true;
                    _currentResponse = null;
                }
            });
        };

        _multiModalLiveClient.Disconnected += (sender, payload) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (_currentResponse != null)
                {
                    _currentResponse.IsSpeaking = false;
                    _currentResponse.Text = "Disconnected";
                    _currentResponse.IsDisconnected = true;
                    _currentResponse = null;
                }
                else
                {
                    ModelResponses.Insert(0, new ModelResponse("Disconnected") { IsDisconnected = true });
                }

                _multiModalLiveClient = null;
                btnStartChat.Content = AppConstants.StartChatText;
            });
        };
    }

    /// <summary>
    /// Starts recording audio from the specified device.
    /// </summary>
    /// <param name="device">The name of the audio input device.</param>
    private void StartRecording(string device)
    {
        if (string.IsNullOrEmpty(device))
            throw new ArgumentException("Please select an audio device."); // More specific exception

        var index = AvailableDevices.IndexOf(device);
        _audioHelper.StartRecording(index, AppConstants.AudioSampleRate, AppConstants.AudioChannels);
        this.IsRecording = true;
    }


    /// <summary>
    /// Stops the current chat session, including audio recording and playback.  Handles potential exceptions.
    /// </summary>
    private async Task StopChat()
    {
        try
        {
            btnStartChat.IsEnabled = false;
            btnStartChat.Content = "Stopping...";

            _cancellationTokenSource?.Cancel();
            _audioHelper.StopPlayback();
            _audioHelper.StopRecording();
            _audioHelper.ClearPlayback(); // Consider if this is needed twice

            this.IsRecording = false;
            if (_multiModalLiveClient != null)
            {
                await _multiModalLiveClient.DisconnectAsync(); // Consider adding a timeout
                _multiModalLiveClient = null;
            }

            btnStartChat.Content = AppConstants.StartChatText;
        }
        catch (Exception ex)
        {
            MessageBox.Show("An error occurred while stopping the chat: " + ex.Message, "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            btnStartChat.IsEnabled = true;
        }
    }

    /// <summary>
    /// Event handler for receiving audio data from the NAudioHelper.
    /// </summary>
    private async void AudioDataReceivedEventHandler(object? sender, byte[] e)
    {
        try
        {
            if (_multiModalLiveClient != null)
            {
                Dispatcher.Invoke(() => { progressBarMicLevel.Value = _audioHelper.GetMicLevel(e); });
                await _multiModalLiveClient.SendAudioAsync(e, AppConstants.AudioContentType,
                    _cancellationTokenSource?.Token ?? default); // Use null-conditional operator and default for safety
            }
            else
            {
                _audioHelper.StopPlayback(); // Stop audio if there's no client.
                _audioHelper.StopRecording();
                this.IsRecording = false;
            }
        }
        catch (OperationCanceledException)
        {
            //It is okay, do nothing.
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
                MessageBox.Show("Error sending audio data: " + ex.Message, "Audio Error")); // Show error on UI thread.
        }
    }

    /// <summary>
    /// Event handler for the window closing event.  Ensures resources are released.
    /// </summary>
    private async void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        await StopChat();
        
    }

    #region INotifyPropertyChanged

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

    #endregion

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
       Process.GetCurrentProcess().Kill();
    }
}
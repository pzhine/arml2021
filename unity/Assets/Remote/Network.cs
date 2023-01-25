namespace WorldAsSupport.Remote {
  /// <summary>
  /// Base class for a network node that sends or receives remote-control commands
  /// </summary>
  public class Network {
    public static int PORT = 1980;
    public static int BUFFER_SIZE = 52428800; // 50 mb
    public string Name;
    public bool IsConnected = false;
    
    public delegate void Error(string message);
    public event Error OnError;

    public delegate void StatusUpdate(string message);
    public event StatusUpdate OnStatusUpdate;

    public virtual void Initialize(string networkName) {
      this.Name = networkName;
    }

    protected void RaiseError(string message) {
      if (this.OnError != null) {
        MainThreadDispatcher.Instance().Enqueue(() => this.OnError(message));
      }
    }

    protected void RaiseStatusUpdate(string message) {
      if (this.OnStatusUpdate != null) {
        MainThreadDispatcher.Instance().Enqueue(() => this.OnStatusUpdate(message));
      }
    }
  }
}
using System.Timers;
using Timer = System.Timers.Timer;
namespace OpenDSM.Core.Handlers;

/// <summary>
/// The basic <seealso cref="System.Timers.Timer">Timer</seealso> with extra functionallity
/// </summary>
public class TimerPlus : IDisposable
{
    private readonly ElapsedEventHandler _action;
    private Timer _timer;
    private TimeSpan _period;
    private DateTime _next;

    public TimerPlus(long milliseconds, bool auto_reset, bool auto_start, ElapsedEventHandler elapsed)
    {
        _timer = new(milliseconds)
        {
            Enabled = true,
            AutoReset = auto_reset,
        };
        _action = elapsed;
        _timer.Elapsed += _action;
        _period = TimeSpan.FromMilliseconds(milliseconds);
        _next = DateTime.Now.AddMilliseconds(milliseconds);
        if (auto_start)
            Start();

    }
    /// <summary>
    /// The duration of the timer
    /// </summary>
    public TimeSpan Period => _period;
    /// <summary>
    /// When the timer will elapse next
    /// </summary>
    public DateTime Next => _next;
    /// <summary>
    /// How long until the timer elapses
    /// </summary>
    public TimeSpan DueTime => _next - DateTime.Now;
    /// <summary>
    /// Starts the timer
    /// </summary>
    public void Start() => _timer.Start();
    /// <summary>
    /// Stops timer without rasing elapsed event
    /// </summary>
    public void Stop() => _timer.Stop();

    /// <summary>
    /// Adds time to the timer
    /// </summary>
    /// <param name="milliseconds"></param>
    public void Push(long milliseconds)
    {
        bool auto_reset = _timer.AutoReset;
        Stop();
        _period = TimeSpan.FromMilliseconds(milliseconds) + _period;
        _next = DateTime.Now.AddMilliseconds(_period.TotalMilliseconds);
        _timer = new(_period.TotalMilliseconds)
        {
            AutoReset = auto_reset,
            Enabled = true,
        };
        Start();
    }
    /// <summary>
    /// Restarts the timer without rasing the elapsed event
    /// </summary>
    public void ResetWithoutElapsing()
    {
        bool auto_reset = _timer.AutoReset;
        Stop();
        _next = DateTime.Now.AddMilliseconds(_period.TotalMilliseconds);
        _timer = new(_period.TotalMilliseconds)
        {
            AutoReset = auto_reset,
            Enabled = true,
        };
        Start();
    }

    /// <summary>
    /// Returns the underlying <seealso cref="System.Timers.Timer">Timer</seealso>
    /// </summary>
    /// <returns></returns>
    public Timer GetBaseTimer() => _timer;

    /// <summary>
    /// Disposes all the resources associated with this component.
    /// </summary>
    public void Dispose() => _timer.Dispose();
}
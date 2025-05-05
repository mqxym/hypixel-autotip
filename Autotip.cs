//MCCScript 1.0

MCC.LoadBot(new Autotip());

//MCCScript Extensions

/// <summary>
/// Autotip ChatBot plugin for MCCScript.
/// Automatically tips players in specified game modes after joining the lobby,
/// and restarts the tipping cycle after a random delay between configurable minimum
/// and maximum intervals (default: 31–33 minutes).
/// </summary>
public class Autotip : ChatBot
{
    #region Constants

    /// <summary>
    /// The username of the tipping player. Replace with your in-game name.
    /// </summary>
    private const string TipUsername = "your_name";

    /// <summary>
    /// The number of game ticks to wait before starting to send tips.
    /// </summary>
    private const int StartRunTicks = 50;

    /// <summary>
    /// The number of game ticks to wait after the initial tips before retrying failed modes.
    /// </summary>
    private const int EndRunTicks = 50;

    /// <summary>
    /// The number of game ticks between each individual tip command.
    /// </summary>
    private const int TipDelayTicks = 10;

    /// <summary>
    /// The maximum number of supported game modes for tipping.
    /// </summary>
    private const int MaxGameModes = 10;

    /// <summary>
    /// The minimum delay before starting the next tipping round, in seconds.
    /// Default corresponds to 31 minutes.
    /// </summary>
    private const int MinIntervalSeconds = 31 * 60;

    /// <summary>
    /// The maximum delay before starting the next tipping round, in seconds.
    /// Default corresponds to 33 minutes.
    /// </summary>
    private const int MaxIntervalSeconds = 33 * 60;

    #endregion

    #region Fields

    /// <summary>
    /// Cumulative tick counter used to schedule actions.
    /// </summary>
    private int _tickCount = 0;

    /// <summary>
    /// Number of tips sent in the initial tipping wave.
    /// </summary>
    private int _initialTipCount = 0;

    /// <summary>
    /// Number of tips sent in the retry (failed) tipping wave.
    /// </summary>
    private int _failedTipCount = 0;

    /// <summary>
    /// Target number of tips to send in the initial wave (up to 9 or number of players).
    /// </summary>
    private int _initialTipTarget = 9;

    /// <summary>
    /// Flag indicating whether the initial tipping wave has completed.
    /// </summary>
    private bool _isInitialTippingComplete = false;

    /// <summary>
    /// List of game mode identifiers that failed to tip on first attempt.
    /// </summary>
    private readonly List<string> _failedGameModes = new List<string>();

    /// <summary>
    /// Cached array of currently online player names (excluding the using player itself).
    /// </summary>
    private string[] _onlinePlayers = Array.Empty<string>();

    /// <summary>
    /// Supported game modes for tipping, in the order they are processed.
    /// </summary>
    private readonly string[] _gameModes =
        { "bsg", "mw", "tnt", "cops", "uhc", "war", "sw", "smash", "classic", "arcade" };

    /// <summary>
    /// The DateTime when the current tipping round was started.
    /// Used to calculate elapsed time for the next round trigger.
    /// </summary>
    private DateTime _lastRoundStart = DateTime.MinValue;

    /// <summary>
    /// Random number generator to pick a delay interval between rounds.
    /// </summary>
    private readonly Random _random = new Random();

    /// <summary>
    /// The currently selected random interval (as a TimeSpan) for this round.
    /// </summary>
    private TimeSpan _currentInterval;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the list of online players available to tip,
    /// excluding the players's own username.
    /// </summary>
    private string[] OnlinePlayers => _onlinePlayers;

    #endregion

    #region Override Methods

    /// <summary>
    /// Processes incoming chat messages to detect tip failures.
    /// Parses messages indicating "player not online" and maps them
    /// to the corresponding game mode based on the tick count.
    /// </summary>
    /// <param name="text">Raw chat text received from server.</param>
    public override void GetText(string text)
    {
        string processedText = GetVerbatim(text);

        if (processedText.Contains("That player is not online", StringComparison.OrdinalIgnoreCase))
        {
            // Ignore failures that occur outside the expected tipping window
            if (_tickCount > (StartRunTicks + TipDelayTicks * 10 + MaxGameModes * 6 * 10))
                return;

            // Determine which game mode's tip failed based on tick timing
            for (int i = 0; i < MaxGameModes; i++)
            {
                int lowerBound = StartRunTicks + TipDelayTicks * 10 + i * 6 * 10;
                int upperBound = lowerBound + 20; // ~2-second window

                if (_tickCount >= lowerBound && _tickCount <= upperBound)
                {
                    _failedGameModes.Add(_gameModes[i]);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Called on every game tick. Manages the timing of tipping waves,
    /// booster tips, and the scheduling of new tipping rounds.
    /// </summary>
    public override void Update()
    {
        // Initialize round timer and state on first invocation
        if (_lastRoundStart == DateTime.MinValue)
        {
            _lastRoundStart = DateTime.Now;
            ResetTippingState();
        }

        // If elapsed time exceeds the random interval, start a new round
        if (DateTime.Now - _lastRoundStart >= _currentInterval)
        {
            _lastRoundStart = DateTime.Now;
            ResetTippingState();
            return;
        }

        // Increment the internal tick counter
        _tickCount++;

        // Tip all boosters approximately at 15 minutes after TipWave
        if (_tickCount == 900 * 10)
        {
            TipAll();
        }

        // Once past the startup delay, perform tipping logic
        if (_tickCount >= StartRunTicks)
        {
            if (_isInitialTippingComplete)
            {
                HandleFailedTipping();
            }
            else
            {
                if (_tickCount == StartRunTicks)
                {
                    SendText("/lobby bedwars");
                }
                HandleInitialTipping();
            }
        }
    }

    #endregion

    #region Reset Logic

    /// <summary>
    /// Resets all counters, flags, and player lists for a fresh tipping round.
    /// Also selects a new random interval for the next round within configured bounds.
    /// </summary>
    private void ResetTippingState()
    {
        _tickCount = 0;
        _initialTipCount = 0;
        _failedTipCount = 0;
        _initialTipTarget = 9;
        _isInitialTippingComplete = false;
        _failedGameModes.Clear();
        _onlinePlayers = Array.Empty<string>();

        // Choose a random delay between MinIntervalSeconds and MaxIntervalSeconds
        int seconds = _random.Next(MinIntervalSeconds, MaxIntervalSeconds + 1);
        _currentInterval = TimeSpan.FromSeconds(seconds);

        LogToConsole($"Next tipping round in {seconds / 60}m {seconds % 60}s");
    }

    #endregion

    #region Tipping Waves

    /// <summary>
    /// Handles the initial tipping wave across configured game modes.
    /// Sends one tip every TipDelayTicks until the target or player list is exhausted.
    /// </summary>
    private void HandleInitialTipping()
    {
        int tipTriggerTick = StartRunTicks + TipDelayTicks * 10 + _initialTipCount * 6 * 10;

        if (_tickCount == tipTriggerTick)
        {
            // On first tip, gather online players and adjust target count
            if (_initialTipCount == 0)
            {
                _onlinePlayers = GetOnlinePlayers()
                    .Where(p => !string.Equals(p, TipUsername, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                _initialTipTarget = Math.Min(9, _onlinePlayers.Length - 1);
            }

            if (_initialTipCount <= _initialTipTarget)
            {
                TipPlayer(_onlinePlayers[_initialTipCount], _gameModes[_initialTipCount]);
                _initialTipCount++;
            }
            else
            {
                LogToConsole("Tipping complete!");
                if (_failedGameModes.Count > 0)
                {
                    LogToConsole($"Retrying {_failedGameModes.Count} failed modes");
                }
                _initialTipCount = 0;
                _isInitialTippingComplete = true;
            }
        }
    }

    /// <summary>
    /// Handles retrying tips for any game modes that failed in the initial wave.
    /// Sends one tip every TipDelayTicks until all failed modes have been retried.
    /// </summary>
    private void HandleFailedTipping()
    {
        if (_failedGameModes.Count == 0) return;

        // On first retry, refresh the online players list
        if (_failedTipCount == 0)
        {
            _onlinePlayers = GetOnlinePlayers()
                .Where(p => !string.Equals(p, TipUsername, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        int tipTriggerTick = StartRunTicks + EndRunTicks + TipDelayTicks * 10
                             + MaxGameModes * 6 * 10 + _failedTipCount * 6 * 10;

        if (_tickCount == tipTriggerTick && _onlinePlayers.Length > _failedGameModes.Count)
        {
            TipPlayer(_onlinePlayers[_failedTipCount], _failedGameModes[_failedTipCount]);
            _failedTipCount++;

            // If all retries are done, clear error list and reset counter
            if (_failedTipCount == _failedGameModes.Count)
            {
                _failedTipCount = 0;
                _failedGameModes.Clear();
            }
        }
    }

    #endregion

    #region Tip Commands

    /// <summary>
    /// Sends a tip command to a specific player for a specified game mode.
    /// </summary>
    /// <param name="playerName">The target player's username.</param>
    /// <param name="gameName">The game mode to tip for.</param>
    private void TipPlayer(string playerName, string gameName)
    {
        SendText($"/tip {playerName} {gameName}");
    }

    /// <summary>
    /// Sends a tip command to all eligible booster players.
    /// </summary>
    private void TipAll()
    {
        SendText("/tipall");
    }

    #endregion
}
//MCCScript 1.0

MCC.LoadBot(new Autotip());

//MCCScript Extensions

/// <summary>
/// Autotip ChatBot plugin for MCCScript.
/// Automatically tips players in specified game modes after joining the lobby.
/// </summary>
public class Autotip : ChatBot
{
    #region Constants

    /// <summary>
    /// The username of the player. Replace "your_name" with your actual username.
    /// </summary>
    private const string TipUsername = "your_name";

    /// <summary>
    /// The initial delay before starting the tipping process (in ticks).
    /// </summary>
    private const int StartRunTicks = 50;

    /// <summary>
    /// The delay after the initial run before resetting (in ticks).
    /// </summary>
    private const int EndRunTicks = 50;

    /// <summary>
    /// The delay between each tip action (in ticks).
    /// </summary>
    private const int TipDelayTicks = 10;

    /// <summary>
    /// The maximum number of game modes available for tipping.
    /// </summary>
    private const int MaxGameModes = 10;

    #endregion

    #region Fields

    /// <summary>
    /// Counter to keep track of ticks since the plugin started.
    /// </summary>
    private int _tickCount = 0;

    /// <summary>
    /// Counter for the number of tips sent in the initial tipping wave.
    /// </summary>
    private int _initialTipCount = 0;

    /// <summary>
    /// Counter for the number of tips sent in the failed tipping wave.
    /// </summary>
    private int _failedTipCount = 0;

    /// <summary>
    /// Target number of tips to send in the initial tipping wave.
    /// </summary>
    private int _initialTipTarget = 9;

    /// <summary>
    /// Flag indicating whether the initial tipping process is complete.
    /// </summary>
    private bool _isInitialTippingComplete = false;

    /// <summary>
    /// List to store game modes where tipping failed.
    /// </summary>
    private readonly List<string> _failedGameModes = new List<string>();

    /// <summary>
    /// Array of online players excluding the autotip player itself.
    /// </summary>
    private string[] _onlinePlayers = Array.Empty<string>();

    /// <summary>
    /// Array of game modes available for tipping.
    /// </summary>
    private readonly string[] _gameModes =
        { "bsg", "mw", "tnt", "cops", "uhc", "war", "sw", "smash", "classic", "arcade" };

    #endregion

    #region Properties

    /// <summary>
    /// Gets the list of online players excluding the tipping player.
    /// </summary>
    private string[] OnlinePlayers => _onlinePlayers;

    #endregion

    #region Override Methods

    /// <summary>
    /// Processes incoming text messages to identify failed tipping attempts.
    /// </summary>
    /// <param name="text">The text message received.</param>
    public override void GetText(string text)
    {
        // Extract the verbatim text to ensure accurate processing.
        string processedText = GetVerbatim(text);

        // Check if the message indicates that a player is not online.
        if (processedText.Contains("That player is not online", StringComparison.OrdinalIgnoreCase))
        {
            // Ignore failures that occur late in the tipping process.
            if (_tickCount > (StartRunTicks + TipDelayTicks * 10 + 10 * 6 * 10))
            {
                return;
            }

            // Determine which game mode failed based on the current tick count.
            for (int i = 0; i < MaxGameModes; i++)
            {
                int lowerBound = StartRunTicks + TipDelayTicks * 10 + i * 6 * 10;
                int upperBound = lowerBound + 20; // 2 seconds window for response

                if (_tickCount >= lowerBound && _tickCount <= upperBound)
                {
                    _failedGameModes.Add(_gameModes[i]);
                    break;
                }
            }
        }

        // Optional: Log the received text for debugging purposes.
        // LogToConsole(processedText);
    }

    /// <summary>
    /// Updates the plugin state on each tick.
    /// </summary>
    public override void Update()
    {
        _tickCount++;

        // Tip all active boosters every 15 and ~45 minutes
        if ( (_tickCount == 900 * 10) || (_tickCount == 2710 * 10) ) {
            TipAll();
        }

        if (_tickCount >= StartRunTicks)
        {
            if (_isInitialTippingComplete)
            {
                HandleFailedTipping();

                // Reset the tipping process after approximately 3700 seconds.
                if (_tickCount >= 3700 * 10)
                {
                    _isInitialTippingComplete = false;
                    _tickCount = 0;
                }

                return;
            }

            // Join the Bedwars lobby after the initial delay.
            if (_tickCount == StartRunTicks)
            {
                SendText("/lobby bedwars");
            }

            // Handle the initial tipping wave.
            HandleInitialTipping();
        }
    }

    #endregion

    #region Tipping Methods

    /// <summary>
    /// Handles the initial wave of tipping players.
    /// </summary>
    private void HandleInitialTipping()
    {
        int tipTriggerTick = StartRunTicks + TipDelayTicks * 10 + _initialTipCount * 6 * 10;

        if (_tickCount == tipTriggerTick)
        {
            // Initialize the list of online players on the first tip.
            if (_initialTipCount == 0)
            {
                _onlinePlayers = GetOnlinePlayers()
                                 .Where(player => !string.Equals(player, TipUsername, StringComparison.OrdinalIgnoreCase))
                                 .ToArray();

                // Set the target number of tips based on available players.
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
                    LogToConsole($"Now tipping the {_failedGameModes.Count} failed gamemodes");
                }

                // Reset for the failed tipping wave.
                _initialTipCount = 0;
                _isInitialTippingComplete = true;
            }
        }
    }

    /// <summary>
    /// Handles tipping players in failed game modes.
    /// </summary>
    private void HandleFailedTipping()
    {
        if (_failedGameModes.Count > 0)
        {
            // Initialize the list of online players if not already done.
            if (_failedTipCount == 0)
            {
                _onlinePlayers = GetOnlinePlayers()
                                 .Where(player => !string.Equals(player, TipUsername, StringComparison.OrdinalIgnoreCase))
                                 .ToArray();
            }

            int tipTriggerTick = StartRunTicks + EndRunTicks + TipDelayTicks * 10 + 10 * 6 * 10 + _failedTipCount * 6 * 10;

            if (_tickCount == tipTriggerTick)
            {
                // Ensure there are enough online players to tip.
                if (_onlinePlayers.Length > _failedGameModes.Count)
                {
                    TipPlayer(_onlinePlayers[_failedTipCount], _failedGameModes[_failedTipCount]);
                    _failedTipCount++;

                    // Reset if all failed game modes have been tipped.
                    if (_failedTipCount == _failedGameModes.Count)
                    {
                        _failedTipCount = 0;
                        _failedGameModes.Clear();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sends a tip command to a specified player for a given game mode.
    /// </summary>
    /// <param name="playerName">The name of the player to tip.</param>
    /// <param name="gameName">The name of the game mode to tip for.</param>
    private void TipPlayer(string playerName, string gameName)
    {
        // Construct the tip command.
        string tipCommand = $"/tip {playerName} {gameName}";

        // Optional: Log the tip action for debugging purposes.
        // LogToConsole($"Tipping player '{playerName}' for game mode '{gameName}'.");

        SendText(tipCommand);
    }

    /// <summary>
    /// Sends a tip all command to the server.
    /// </summary>
    private void TipAll()
    {
        // Construct the tip command.
        string tipCommand = "/tipall";


        SendText(tipCommand);
    }

    #endregion
}
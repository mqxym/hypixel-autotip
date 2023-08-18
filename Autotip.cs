//MCCScript 1.0

﻿MCC.LoadBot(new Autotip());

//MCCScript Extensions

class Autotip : ChatBot
{

    private readonly string username = "wobblterror";
    private int count = 0;
    private readonly int startrun = 50;
    private readonly int endrun = 50;
    private readonly int tipDelay = 10;
    private int tipCount = 0;
    private int tipCountEnd = 0;
    private int tipCountTarget = 9;
    private bool tippingComplete = false;
    private readonly string[] games = new string[] { "bsg", "mw", "tnt", "cops", "uhc", "war", "sw", "smash", "classic", "arcade" };
    private string[] OnlinePlayers { get; set; }
    private List<string> FailedGamemodes { get; set; } = new List<string>();

    public override void GetText(string text)
    {
        text = GetVerbatim(text);
        if (text.Contains("That player is not online", StringComparison.OrdinalIgnoreCase))
        {
            int countCheck = count;
            //Ignore it when the fail happens in the end-round
            if (countCheck > (startrun + tipDelay * 10 + 10 * 6 * 10))
            {
                return;
            }
            LogToConsole(countCheck);
            //checks which gamemode the player failed to tip
            for (int i = 0; i <= 9 ; i++)
            {
                if (countCheck >= startrun + tipDelay*10 + i*6*10 //Count >= e.g. 150
                    && countCheck <= startrun + tipDelay*10 + i*6*10 + 20) //Count <= e.g. 170 (2 sec for answer)
                {
                    FailedGamemodes.Add(games[i]);
                    LogToConsole(games[i]);
                    break;
                }
            }
        }
        //LogToConsole(text);
    }

    public override void Update()
    {
        count++;

        if (count >= startrun)
        {

            if (tippingComplete)
            {
                //tip failed gamemodes 
                if (FailedGamemodes.Count > 0)
                {
                    //get filtered list of online Players to start the failed tips wave
                    if (tipCountEnd == 0)
                    {
                        OnlinePlayers = GetOnlinePlayers().Where(item => item != username).ToArray();
                    }
                    
                    if (count == (startrun + endrun + tipDelay * 10 + 10 * 6 * 10 + tipCountEnd * 6 * 10))
                    {
                        //if enough players are available
                        if (OnlinePlayers.Length > FailedGamemodes.Count)
                        {
                            TipPlayer(OnlinePlayers[tipCountEnd], FailedGamemodes[tipCountEnd]);
                            tipCountEnd++;
                            
                            //reset the list
                            if (tipCountEnd == FailedGamemodes.Count)
                            {
                                tipCountEnd = 0;
                                FailedGamemodes.Clear();
                            }
                        }
                    }
                }
                //After ~3700 seconds reset the counter and tip again
                if (count >= 3700 * 10)
                {
                    tippingComplete = false;
                    count = 0;
                }
                else
                {
                    return;
                }
            }
            //After 5 seconds join bedwars lobby
            if (count == startrun)
            {
                SendText("/lobby bedwars");
            }
            //After 15 seconds tip the first player, wait tipDelay and tip the next player
            if (count == (startrun + tipDelay * 10 + tipCount * 6 * 10))
            {
                //get filtered list of online Players at the first tip wave
                if (tipCount == 0)
                {
                    OnlinePlayers = GetOnlinePlayers().Where(item => item != username).ToArray();
                    //do 10 tips or the ammount of players in the lobby
                    tipCountTarget = Math.Min(9, OnlinePlayers.Length - 1);
                }

                if (tipCount <= tipCountTarget)
                {
                    TipPlayer(OnlinePlayers[tipCount], games[tipCount]);
                    tipCount++;
                } 
                else 
                {
                    LogToConsole("Tipping complete!");
                    LogToConsole(FailedGamemodes.Count + " failed gamemodes");
                    tipCount = 0;
                    tippingComplete = true;
                }
            }
        }
    }

    private void TipPlayer(string playerName, string gameName)
    {
        //LogToConsole("Tip " + playerName + " " + gameName);
        SendText("/tip " + playerName + " " + gameName);
    }
}
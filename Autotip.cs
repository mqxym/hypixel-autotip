//MCCScript 1.0

﻿MCC.LoadBot(new Autotip());

//MCCScript Extensions

class Autotip : ChatBot
{

    /*
        IDEAS:
        - Use another player when target player has gone offline

    */

    private readonly string username = "wobblterror";
    private int count = 0;
    private readonly int startrun = 50;
    private readonly int tipDelay = 10;
    private int tipCount = 0;
    private int tipCountTarget = 9;
    private bool tippingComplete = false;
    private readonly string[] games = new string[] { "bsg", "mw", "tnt", "cops", "uhc", "war", "sw", "smash", "classic", "arcade" };
    private string[] onlinePlayers { get; set; }

    public override void Update()
    {
        count++;

        if (count >= startrun)
        {

            if (tippingComplete)
            {
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
                if (count == startrun + tipDelay * 10)
                {
                    onlinePlayers = GetOnlinePlayers().Where(item => item != username).ToArray();
                    //do 10 tips or the ammount of players in the lobby
                    tipCountTarget = Math.Min(9, onlinePlayers.Length - 1);
                }

                if (tipCount <= tipCountTarget)
                {
                    TipPlayer(onlinePlayers[tipCount], games[tipCount]);
                    tipCount++;
                } 
                else 
                {
                    tipCount = 0;
                    tippingComplete = true;
                }
            }
        }
    }

    private void TipPlayer(string playerName, string gameName)
    {
        LogToConsole("Tip " + playerName + " " + gameName);
        //SendText("/tip " + playerName + " " + gameName);
    }
}
//MCCScript 1.0

﻿MCC.LoadBot(new Autotip());

//MCCScript Extensions

class Autotip : ChatBot
{

        private int count = 0;
        private readonly int startrun = 50;
        private readonly int tipDelay = 10;
        private int tipCount = 0;
        private bool tippingComplete = false;
        private readonly string[] games = new string[] {"bsg", "mw", "tnt", "cops", "uhc", "war", "sw", "smash" , "classic"};
        private string[] onlinePlayers { get; set; }

        public override void Update()
        {
            count++;

            if (count >= startrun)
            {

                if (tippingComplete)
                {
                    //After ~3700 seconds reset the counter and tip again
                    if (count >= 3700*10)
                    {
                        tippingComplete = false;
                        count = 0;
                    } else
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
                if (count == startrun + tipDelay*10)
                {
                    onlinePlayers = GetOnlinePlayers();
                    if (tipCount < onlinePlayers.Length)
                    {
                        TipPlayer(onlinePlayers[tipCount], games[tipCount]);
                    }
                    
                    tipCount++;
                } else if(count == (startrun + tipDelay*10 + tipCount*6*10)) 
                {
                    if (tipCount < onlinePlayers.Length)
                    {
                        TipPlayer(onlinePlayers[tipCount], games[tipCount]);
                    }
                    tipCount++;

                    //After 9 tips finish the round
                    if(tipCount > 8)
                    {
                        tipCount = 0;
                        tippingComplete = true;
                    }
                }
            }
        }

        private void TipPlayer(string playerName, string gameName)
        {
            SendText("/tip " + playerName + " " + gameName);
        }
    }

using System;
using System.Collections.Generic;
using System.Xml;
using GameEngine;
using System.Threading;
using System.Text;

namespace GameEngine
{
    public enum ePlayType
    {
        Local,
        Bingo,
        PullTab,
        Raffle,
    }
    public abstract class SlotEngine //: GameEngine.GameEngine, IGameEngine
    {
        public enum STAGE
        {
            STAGE_IDLE,
            STAGE_STARTGAME,
            STAGE_FREESPIN,
            STAGE_BONUS,
            STAGE_SLOTFEATURE,
        }

        protected STAGE m_Stage;
        protected ePlayType m_ePlayType;
        protected Bonus m_CurrentBonusGame;
        protected SlotFeature m_CurrentSlotFeature;
        protected XmlDocument m_xmlDocument;
        //protected SlotReel m_BaseSlot;
        //protected SlotReel m_FreespinSlot;
        protected SlotReel m_CurrentSlotReel;
        protected Dictionary<int,SlotReel> m_SlotReels;
        //protected int m_iTotalWinAmount;

        protected SpinResult m_SpinResult;
        protected List<Bonus> m_BonusList;
        //protected List<SlotFeature> m_SlotFeatures;
        //protected List<List<int>> m_FSReelStops;
        //protected List<int> m_BaseReelStops;

        protected Queue<SlotFeature> m_TriggeredSlotFeatures;

        protected int m_iBetLevel;
        protected int m_iBetCredits;
        //protected int m_iBaseWinAmount;
        protected bool m_bRecover;
        protected bool m_bWayPay;

        public bool m_bForceSpin = false;
        public string m_sForceStop = "";

        public bool m_bFSForceSpin = false;
        public string m_sFSForceStop = "";

        //protected string m_sMarketType;
        protected string m_sDefaultReelStops;

        protected string m_sTicket;

        protected static Action<string, string> m_RecoveryData;
        private static Action<string> m_Log;
        private static Action<string> m_ThrowErrorAction;


        public SlotEngine()
        {
            m_BaseThread = null;
            m_ePlayType = (ePlayType)0;
            if (m_ePlayType == ePlayType.Bingo)
            {
                m_ePlayType = ePlayType.PullTab;
            }
            m_TriggeredSlotFeatures = new Queue<SlotFeature>();
            m_SpinResult = new SpinResult();
            m_BonusList = new List<Bonus>();
            RNG.Random.CreateRandom();
           // m_sMarketType = "general";

            m_Stage = STAGE.STAGE_IDLE;
            m_RecoveryData = null;
            m_bRecover = false;
            m_SlotReels = new Dictionary<int, SlotReel>();
        }

        public static void SetRecoveryData(string name, string sData)
        {
            if (m_RecoveryData != null)
            {
                m_RecoveryData(name, sData);
            }
        }

        public bool WayPay()
        {
            return m_bWayPay;
        }

        public static void ThrowError(string sErrorMessage)
        {
            if (m_ThrowErrorAction != null)
            {
                m_ThrowErrorAction(sErrorMessage);
            }
#if _SIMULATOR
            else
            {
                throw new Exception(sErrorMessage);
            }
#endif

        }

        public static void setLogAction(string sLogMessage)
        {
            if (m_Log != null)
            {
                m_Log(sLogMessage);
            }
        }

        public void setActionRecoveryData(Action<string, string> rData)
        {
            m_RecoveryData = rData;

            for (int i = 0; i < m_BonusList.Count; i++)
            {
                m_BonusList[i].setActionRecoveryData(rData);
            }
        }

        public void setActionLog(Action<string> logAction)
        {
            m_Log = logAction;
            Log("GAME ENGINE TEST!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        public static void Log(string sLog)
        {
            if (m_Log != null)
                m_Log(sLog);
        }

        public void setRecoveryData(string sKey, string sData)
        {
            if (m_RecoveryData != null)
                m_RecoveryData(sKey, sData);
        }

        public virtual void LoadXML(string sXMLFileName)
        {
            m_xmlDocument = new XmlDocument();
            try
            {
                m_xmlDocument.Load(sXMLFileName);
            }
            catch (Exception e)
            {
                ThrowError("Game Engine Error!! Load Paytable xml error");
                Log("Game Engine Error: SlotEngine::LoadXML() - " + e.Message);
            }
            XmlNode engineNode = m_xmlDocument.SelectSingleNode("ENGINE");
            XmlNodeList childNodes = engineNode.ChildNodes;

            m_iBetCredits = int.Parse(engineNode.Attributes["credits"].Value);

            if (engineNode.Attributes["waypay"] != null)
            {
                m_bWayPay = bool.Parse(engineNode.Attributes["waypay"].Value);
            }

            for (int i = 0; i < childNodes.Count; i++)
            {
                XmlNode node = childNodes[i];
                switch (node.Name)
                {
                    case "SLOT":
                        {
                            int iSlotId = int.Parse(node.Attributes["id"].Value);
                            SlotReel reel = new SlotReel(iSlotId, node, m_bWayPay);
                            reel.LoadXML(node);
                            m_SlotReels.Add(iSlotId, reel);
                            break;
                        }
                }
            }
            m_CurrentSlotReel = m_SlotReels[0];
        }

        public string getBaseDefaultReelStops()
        {
            SpinResult result = m_CurrentSlotReel.PlayGame();
            while (result.getWinAmount() != 0)
            {
                result = m_CurrentSlotReel.PlayGame();
            }
            string sReels = "";
            int[] reels = result.getReelStops();
            for (int i = 0; i < reels.Length; i++)
            {
                if (i != 0) sReels += " ";
                sReels += reels[i];
            }
            return sReels;
        }

        public int getBetCredits()
        {
            return m_iBetCredits;
        }

        public Action<SpinResult> m_GameResult;
        public Action<BonusResult> m_BonusResult;


        public virtual void GameResults(SpinResult e)
        {
            //base.GameResults(e);
            if (m_Stage == STAGE.STAGE_FREESPIN)
            {
                if (!m_SpinResult.FreePlay)
                {
                    m_Stage = STAGE.STAGE_IDLE;
                    m_CurrentSlotReel = m_SlotReels[0];
                    m_SpinResult.FreePlay = false;
                }
                m_SpinResult.ReadyForSpin = ReadyForSpin();//true;
            }
            m_GameResult?.Invoke(e);
        }

        public void BonusResults(BonusResult e)
        {
            //base.BonusResults(e);
            if (m_BonusResult != null)
            {
                m_BonusResult(e);
            }
        }

        public List<int[]> getReelStrips(bool bFreeSpin = false)
        {
            /* if (!bFreeSpin)
                 return m_BaseSlot.getReelStrips();
             else
                 return m_FreespinSlot.getReelStrips();*/
            return m_CurrentSlotReel.getReelStrips();
        }

        private Thread m_BaseThread;
        public void PlayGame(int betAmount, string sTicket)
        {
            if (m_Stage == STAGE.STAGE_IDLE || m_Stage == STAGE.STAGE_FREESPIN)
            {
                Log("GameEngine Error! IGameEngine::PlayGame() - Start to Play Game ");
                m_iBetLevel = betAmount;
                m_sTicket = sTicket;
                try
                {
                    Log("GameEngine Error! IGameEngine::PlayGame() - Calling play thread ");
                    //Thread play = new Thread(PlayThread);
                    //play.Start(betAmount);
                    m_BaseThread.Start(betAmount);
                }
                catch (Exception e)
                {
                    ThrowError("Game Engine Error!");
                    Log("GameEngine Error! IGameEngine::PlayGame() - " + e.Message);
                }


            }
            else
            {
                ThrowError("GAME ENGINE ERROR!");
                Log("GameEngine Error! SlotEngine::PlayGame() - Stage is neither Idle nor freespin. Stage is in " + m_Stage);
            }
        }

       /* public virtual void ParseTicket(string sTicket)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sTicket);
            XmlNode playNode = doc.SelectSingleNode("PLAY");
            string sReelStops = playNode.Attributes["RS"].Value;
            int[] reelStops = StringUtility.StringToIntArray(sReelStops, ' ');
            m_iTotalWinAmount = int.Parse(playNode.Attributes["TotalWin"].Value) * m_iBetLevel;

            m_SpinResult = m_BaseSlot.PlayTicket(reelStops, m_iBetLevel);
            for (int i = 0; i < playNode.ChildNodes.Count; i++)
            {
                XmlNode childNode = playNode.ChildNodes[i];
                if (childNode.Name == "BONUS")
                {
                    int iBonusId = int.Parse(childNode.Attributes["Type"].Value);
                    Bonus bonusTicket = m_BonusList[iBonusId];
                    bonusTicket.setBetLevel(m_iBetLevel);
                    bonusTicket.InitTicket(childNode);
                }
                else if (childNode.Name == "FREESPINS")
                {
                    m_FreespinSlot.InitFreeStopsTickets(childNode);
                }
            }
        }*/

        public virtual void PlayThread(object betAmount)
        {
            //m_Stage = STAGE.STAGE_STARTGAME;
            m_TriggeredSlotFeatures.Clear();
            m_SpinResult.ResetFlags();
            m_CurrentSlotReel.ResetFlags();
            m_iBetLevel = (int)betAmount;
           /* if (m_ePlayType == ePlayType.PullTab || m_ePlayType == ePlayType.Bingo)
            {
                try
                {
                    ParseTicket(m_sTicket);
                }
                catch (Exception e)
                {
                    ThrowError("GAME ENGINE ERROR! Parse Ticket ");
                    Log("GameEngine Error! SlotEngine::PlayThread() - Parse ticket error " + e.Message);
                }
            }*/

            if (m_ePlayType == ePlayType.Local)
            {
                Log("GameEngine Error! IGameEngine::PlayGame() - Inside play thread ");
                if (!string.IsNullOrEmpty(m_sTicket))
                {
                    try
                    {
                        m_CurrentSlotReel.SetForceSpin(m_sTicket);
                    }
                    catch (Exception e)
                    {
                        ThrowError("GAME ENGINE ERROR! Force Ticket ");
                        Log("GameEngine Error! SlotEngine::PlayThread() - Parse ticket error " + e.Message);
                    }

                }

                try
                {
                    Log("GameEngine Error! IGameEngine::PlayThread() - Call PlayGame ");
                    m_SpinResult = m_CurrentSlotReel.PlayGame((int)betAmount, m_Stage == STAGE.STAGE_FREESPIN ? true : false);
                }
                catch (Exception e)
                {
                    ThrowError("GameEngine ERROR! slot reel play game: " + e.Message);
                }

                try
                {
                    string sReelStops = "";
                    int[] reelStops = m_SpinResult.getReelStops();
                    for (int i = 0; i < reelStops.Length; i++)
                    {
                        if (i != 0) sReelStops += " ";
                        sReelStops += reelStops[i].ToString();
                    }
                    setRecoveryData("BGRS", sReelStops);
                    setRecoveryData("ENGINESTAGE", m_Stage.ToString());
                }
                catch (Exception e)
                {
                    ThrowError("GameEngine ERROR! slot reel play game: " + e.Message);
                    Log("Game Engine Error! " + e.Message + " " + e.StackTrace);
                }

            }

            try
            {
                m_SpinResult.BaseWinAmount = m_SpinResult.getWinAmount();
                setRecoveryData("BGWINAMOUNT", m_SpinResult.BaseWinAmount.ToString());
                // m_iBaseWinAmount = m_SpinResult.BaseWinAmount;
                //m_Stage = STAGE.STAGE_IDLE;
                //m_SpinResult.ReadyForSpin = true;
                //m_SpinResult.FreePlay = false;
            }
            catch (Exception e)
            {
                ThrowError("GameEngine ERROR! BGWINAMOUNT: " + e.Message);
                Log("Game Engine Error! " + e.Message + " " + e.StackTrace);
            }


            try
            {
                PlaySlotFeatures();
            }
            catch (Exception e)
            {
                ThrowError("GameEngine ERROR! In feature");
                Log("SlotEngine::PlayThread() - Error!! " + e.Message);
            }

            Log("GameEngine Error! IGameEngine::PlayThread() - Sending results ");
            GameResults(m_SpinResult);
            //GC.Collect();
#if _SIMULATOR
            m_CurrentSlotReel.CollectStatistics();  //NEED TO RECHECK THIS
#endif

            if (m_BaseThread != null && m_BaseThread.IsAlive)
            m_BaseThread.Join();
        }



        public virtual void PlayGameSimulator(int betAmount = 1)
        {
            m_iBetLevel = 1;
            if (m_Stage == STAGE.STAGE_IDLE || m_Stage == STAGE.STAGE_IDLE)
            {
                //m_FSReelStops.Clear();
                m_SpinResult.ClearTiggerLine();
                m_TriggeredSlotFeatures.Clear();
                m_SpinResult.ResetFlags();

                if (m_sForceStop != "" && m_bForceSpin)
                {
                    try
                    {
                        m_CurrentSlotReel.SetForceSpin(m_sForceStop, true);
                    }
                    catch (Exception e)
                    {
                        ThrowError("GAME ENGINE ERROR! Force Ticket ");
                        Log("GameEngine Error! SlotEngine::PlayThread() - Parse ticket error " + e.Message);
                    }

                }

                else if (m_sFSForceStop != "" && m_bFSForceSpin)
                {
                    try
                    {
                        m_CurrentSlotReel.SetForceSpin(m_sFSForceStop, true);
                    }
                    catch (Exception e)
                    {
                        ThrowError("GAME ENGINE ERROR! Force Ticket ");
                        Log("GameEngine Error! SlotEngine::PlayThread() - Parse ticket error " + e.Message);
                    }
                }

                m_SpinResult = m_CurrentSlotReel.PlayGame(betAmount);
                int[] spinResults = m_SpinResult.getReelStops();

                m_Stage = STAGE.STAGE_STARTGAME;
                m_SpinResult.BaseWinAmount = m_SpinResult.getWinAmount();
                // m_iBaseWinAmount = m_SpinResult.BaseWinAmount;
                m_Stage = STAGE.STAGE_IDLE;
                if(!m_SpinResult.FreePlay)
                {
                    m_Stage = STAGE.STAGE_IDLE;
                    m_SpinResult.FreePlay = false;
                }
                else
                {
                    m_Stage = STAGE.STAGE_FREESPIN;
                }
                m_SpinResult.ReadyForSpin = true;
                

                PlaySlotFeatures();
                PlayBonusGame();

                GameResults(m_SpinResult);
#if _SIMULATOR
                m_CurrentSlotReel.CollectStatistics();  //NEED TO RECHECK THIS
#endif
            }

     /*       else if (m_Stage == STAGE.STAGE_FREESPIN)
            {
                m_SpinResult = m_FreespinSlot.PlayGame((int)betAmount, true);
                List<int> reelStops = new List<int>();

                if (!m_SpinResult.FreePlay)
                {
                    m_Stage = STAGE.STAGE_IDLE;
                    m_SpinResult.FreePlay = false;
                }
                m_SpinResult.ReadyForSpin = true;
                GameResults(m_SpinResult);
            }*/
        }

        public virtual void PlaySlotFeatures()
        {

        }

        public virtual void PlayBonusGame()
        {

        }

    /*    public virtual void FreePlayThread(object betAmount)
        {
            m_SpinResult.ResetSlotFeatures();
            if (m_ePlayType == ePlayType.PullTab || m_ePlayType == ePlayType.Bingo)
            {
                m_SpinResult = m_FreespinSlot.PlayTicket(null, (int)betAmount, true);
            }
            if (m_ePlayType == ePlayType.Local)
            {
                m_SpinResult = m_FreespinSlot.PlayGame((int)betAmount, true);
                string sReelStops = "";
                int[] reelStops = m_SpinResult.getReelStops();
                for (int i = 0; i < reelStops.Length; i++)
                {
                    if (i != 0) sReelStops += " ";
                    sReelStops += reelStops[i].ToString();
                }
                FreeSpinProperties fsProperties = m_SpinResult.FreeSpinProp;
                setRecoveryData("FGRS", sReelStops);
                setRecoveryData("ENGINESTAGE", m_Stage.ToString());
                setRecoveryData("FSTOTALWIN", (fsProperties.WinAmount - m_SpinResult.getWinAmount()).ToString());
                setRecoveryData("CURRENTSPIN", (fsProperties.CurrentFreeSpin - 1).ToString());
                setRecoveryData("TOTALSPIN", fsProperties.FreeSpinsTotal.ToString());

            }

            PlaySlotFeatures();
            PlayBonusGame();
            if (!m_SpinResult.FreePlay)
            {
                m_Stage = STAGE.STAGE_IDLE;
            }
            m_SpinResult.ReadyForSpin = true;
            GameResults(m_SpinResult);
            GC.Collect();
        }*/

        public bool ReadyForSpin()
        {
            if (m_Stage == STAGE.STAGE_FREESPIN || m_Stage == STAGE.STAGE_IDLE)
            {
                return true;
            }
            return false;
        }

        public bool isFreePlay()
        {
            return (m_Stage == STAGE.STAGE_FREESPIN);
        }

        public bool WaitingForPick()
        {
            return /*m_Stage == STAGE.STAGE_PICKME || */m_Stage == STAGE.STAGE_BONUS;
        }

        public virtual void PickThread(object pickNumber)
        {
            if (m_CurrentBonusGame != null)
            {
                try
                {
                    m_CurrentBonusGame.SendPick((int)pickNumber);
                }
                catch (Exception e)
                {
                    ThrowError("GAME ENGINE ERROR!");
                    Log("GAME ENGINE. SlotEngine::PickThread() - " + e.Message);
                }
            }
        }

        public void SendPickSimulator(int iPick)
        {
            if (m_CurrentBonusGame != null)
            {
                m_CurrentBonusGame.SendPick((int)iPick);
            }
        }

        public SlotFeature getSlotFeature(int iFeatureId)
        {
            m_CurrentSlotReel.getSlotFeature(iFeatureId);
            return null;
        }

        public Bonus getBonus(int iBonusid)
        {
            for (int i = 0; i < m_BonusList.Count; i++)
            {
                if (m_BonusList[i].getBonusid() == iBonusid)
                {
                    return m_BonusList[i];
                }
            }
            return null;
        }

        public virtual void SentPick(int iPick)
        {
            if(m_Stage == STAGE.STAGE_BONUS && m_CurrentBonusGame != null)
            {
                m_CurrentBonusGame.SendPick(iPick);
            }
            else if (m_Stage == STAGE.STAGE_SLOTFEATURE && m_CurrentSlotFeature != null)
            {
                m_CurrentSlotFeature.SendPick(iPick);
            }
        }

        public void Recover(Dictionary<string, string> dataList, string sTicket = "", int iBetAmount = 0)
        {
            if (dataList.ContainsKey("ENGINESTAGE"))
            {
                m_Stage = (STAGE)Enum.Parse(typeof(STAGE), dataList["ENGINESTAGE"]);
            }
            if (dataList.ContainsKey("BGRS"))
            {
                int[] reelstops = StringUtility.StringToIntArray(dataList["BGRS"], ' ');
                m_SlotReels[0].Recover(reelstops, iBetAmount);
                m_SpinResult = m_SlotReels[0].PlayGame(iBetAmount);
                //NEW
                m_SpinResult.BaseWinAmount = m_SpinResult.getWinAmount();
            }
            m_iBetLevel = iBetAmount;

            for(int i = 0; i < m_SlotReels.Count; i++)
            {
                m_SlotReels[i].SetSlotFeatureBetLevel(m_iBetLevel);
            }

            if (m_Stage == STAGE.STAGE_STARTGAME)
            {
                m_bRecover = true;
                int[] reelstops = StringUtility.StringToIntArray(dataList["BGRS"], ' ');
                m_SlotReels[0].Recover(reelstops, iBetAmount);
                m_Stage = STAGE.STAGE_IDLE;
            }
            if (m_Stage == STAGE.STAGE_FREESPIN)
            {
                RecoverFreeSpin(dataList);
            }
            else if (m_Stage == STAGE.STAGE_BONUS)
            {
                bool bBonusStarted = false;
                if (dataList.ContainsKey("BONUSSTARTED"))
                {
                    bBonusStarted = bool.Parse(dataList["BONUSSTARTED"]);
                }
                if (bBonusStarted)
                {
                    m_SpinResult.BaseWinAmount = int.Parse(dataList["BGWINAMOUNT"]);
                    //m_iBaseWinAmount = m_SpinResult.BaseWinAmount;
                    m_Stage = STAGE.STAGE_BONUS;
                    RecoverBonusSpin(dataList, iBetAmount);
                }
                else
                {
                    m_bRecover = true;
                    m_Stage = STAGE.STAGE_BONUS;
                    RecoverBonusSpin(dataList, iBetAmount);
                    int[] reelstops = StringUtility.StringToIntArray(dataList["BGRS"], ' ');
                    m_SlotReels[0].Recover(reelstops, iBetAmount);
                    m_Stage = STAGE.STAGE_IDLE;
                }
            }
            else if (m_Stage == STAGE.STAGE_SLOTFEATURE)
            {
                m_SpinResult.BaseWinAmount = m_SpinResult.getWinAmount();
                //m_iBaseWinAmount = m_SpinResult.BaseWinAmount;
            }

            if (m_Stage == STAGE.STAGE_IDLE && dataList.ContainsKey("ENGINEBONUSID"))
            {
                if (dataList.ContainsKey("BONUSSTARTED") && bool.Parse(dataList["BONUSSTARTED"]))
                    RecoverBonusSpin(dataList, iBetAmount);
            }
        }

        public virtual void RecoverFreeSpin(Dictionary<string, string> dataList, int iBetAmount = 0)
        {
            m_Stage = STAGE.STAGE_FREESPIN;
            int id = int.Parse(dataList["SLOTID"]);
            SlotReel FreespinReel = m_SlotReels[id];
            if (dataList.ContainsKey("FGRS") && !string.IsNullOrEmpty(dataList["FGRS"].Trim()))
            {
                m_SpinResult = m_SlotReels[0].PlayGame(iBetAmount);
                int[] reelstops = StringUtility.StringToIntArray(dataList["FGRS"], ' ');
                FreespinReel.Recover(reelstops, iBetAmount);

                int iCurrentSpin = int.Parse(dataList["CURRENTSPIN"]);
                int iFSTotalWin = int.Parse(dataList["FSTOTALWIN"]);
                int iTotalSpin = int.Parse(dataList["TOTALSPIN"]);
                m_SpinResult.ClearTiggerLine();
                m_SpinResult.ReadyForSpin = true;
                m_SpinResult.setBonusId(0, 0, 0);
                m_SpinResult.FreePlay = true;
                m_SpinResult.BaseWinAmount = int.Parse(dataList["BGWINAMOUNT"]);
                FreespinReel.RestoreFreeSpinProperties(iCurrentSpin, iTotalSpin, iFSTotalWin);
                m_SpinResult.FreeSpinProp = FreespinReel.getFreeSpinProperties();
            }
            else
            {
                m_bRecover = true;
                int[] reelstops = StringUtility.StringToIntArray(dataList["BGRS"], ' ');
                m_SlotReels[0].Recover(reelstops, iBetAmount);
                m_Stage = STAGE.STAGE_IDLE;
            }

        }

        public virtual void RecoverBonusSpin(Dictionary<string, string> dataList, int iBetAmount = 0)
        {
            //m_Stage = STAGE.STAGE_BONUS;
            m_SpinResult.setBonus(true);
            int iBonusId = int.Parse(dataList["ENGINEBONUSID"]);

            Bonus bonusGame = getBonus(iBonusId);
            if (bonusGame != null)
            {
                // m_SpinResult = m_BaseSlot.PlayGame(iBetAmount);
                m_CurrentBonusGame = bonusGame;
                m_SpinResult.setBonusId(0, m_CurrentBonusGame.getBonusid(), 0);
                bonusGame.setBetLevel(iBetAmount);
                bonusGame.RecoverBonus(dataList, iBetAmount);


            }
        }

        public SpinResult getRecoverSpinResult()
        {
            if (m_SpinResult != null)
                return m_SpinResult;
            return null;//throw new NotImplementedException();
        }

        public BonusResult getRecoverBonusResult()
        {
            if (m_CurrentBonusGame == null)
                return null;
            BonusResult bResult = m_CurrentBonusGame.getBonusResult();
            if (bResult != null)
            {
                return bResult;
            }
            return null;//throw new NotImplementedException();
        }

        public List<Symbol> getPaytable(bool bFreespin = false)
        {
            if (!bFreespin)
            {
                List<Symbol> symbols = m_SlotReels[0].getPayTable().GetAllSymbols();
                return symbols;
            }
            else
            {
                List<Symbol> symbols = m_SlotReels[1].getPayTable().GetAllSymbols();
                return symbols;
            }
        }

        public List<int[]> getWinningLines(bool bFreespin = false)
        {
            if (!bFreespin)
            {
                List<int[]> lines = m_SlotReels[0].getWinningLines();
                return lines;
            }
            else
            {
                List<int[]> lines = m_SlotReels[1].getWinningLines();
                return lines;
            }
        }

        public List<SlotReel.ScatterSymbol> getScatterList(bool bFreespin = false)
        {
            if (!bFreespin)
            {
                return m_SlotReels[0].getScatterList();
            }
            else
            {
                return m_SlotReels[1].getScatterList();
            }
        }

        public List<SlotReel.TriggerSymbol> getTriggerList(bool bFreespin = false)
        {
            if (!bFreespin)
                return m_SlotReels[0].getTriggerList();
            else
                return m_SlotReels[1].getTriggerList();
        }

        public void setThrowErrorAction(Action<string> errorAction)
        {
            m_ThrowErrorAction = errorAction;
        }

        public void setLogAction(Action<string> logAction)
        {
            //throw new NotImplementedException();
        }

#if _SIMULATOR

        public StringBuilder getStatisticsOutout(int iTotalBetAmount)
        {
            StringBuilder engineStringBuilder = new StringBuilder();
            engineStringBuilder.AppendLine(m_SlotReels[0].getStatisticsOutput(iTotalBetAmount).ToString());
            engineStringBuilder.AppendLine("________________________________________________________________");
            if (m_SlotReels[1] != null)
                engineStringBuilder.AppendLine(m_SlotReels[1].getStatisticsOutput(iTotalBetAmount).ToString());

           /* if (m_SlotFeatures != null)
            {
                for (int i = 0; i < m_SlotFeatures.Count; i++)
                {
                    engineStringBuilder.AppendLine(m_SlotFeatures[i].getStatisticsOutput(iTotalBetAmount).ToString());
                }
            }*/


            for (int i = 0; i < m_BonusList.Count; i++)
            {
                engineStringBuilder.AppendLine(m_BonusList[i].getStatisticsOutput(iTotalBetAmount).ToString());
            }

            return engineStringBuilder;
        }

        public int getTotalWinAmountStatistics()
        {
            int iTotalWinAmount = 0;

            
            iTotalWinAmount += m_SlotReels[0].getTotalWinAmountStatistics();
            if (m_SlotReels[1] != null)
                iTotalWinAmount += m_SlotReels[1].getTotalWinAmountStatistics();

            for (int i = 0; i < m_BonusList.Count; i++)
            {
                iTotalWinAmount += m_BonusList[i].getTotalWinAmountStatistics();
            }
            return iTotalWinAmount;
        }
#endif
    }
}



using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;

namespace GameEngine
{
    public class SymbolStatistics
    {
        public int symbolCount;
        public int iSymbolid;
        public int iTotalWinAmount;
        public Dictionary<int, int> numofSymCountList;
    }

    public class SlotReel
    {
        public struct TriggerSymbol
        {
            public enum TYPE
            {
                BONUS,
                SLOT_FEATURE
            }
            public int iSymbolId;
            public int iNumOfSymbols;
            public int iBonusId;
            public int iFeatureId;
            public TYPE triggerType;
        }

        public struct ScatterSymbol
        {
            public int iSymbolId;
            public int iNumOfSymbols;
            public int iMultiplier;
            public int iCredits;
        }

        private int m_iSlotId;
        private Paytable m_Paytable;
        private Winlines m_Winlines;
        Random m_Random;
        private FreeSpinProperties m_FreeSpinProperties;
        private List<string> m_FreeSpinStopsTickets;

        private List<int> m_WildSymbolsID;
        private List<int[]> m_ReelStripList;
        private List<int[]> m_WinningLines;
        private List<int[]> m_SlotColumns;

        private List<TriggerSymbol> m_TriggerSymbols;
        private List<ScatterSymbol> m_ScatterSymbols;

        protected List<SlotFeature> m_SlotFeatures;

        //private int[] m_iReelStops;
        private List<int> m_iReelStops;
        private List<int> m_iExtraWilds; //Extra wilds bool 1 and 0
        private bool m_bForceSpin;
        private bool m_bWayPay;

        private SpinResult m_SpinResult;

        private bool m_bRecovery;
        private int[] m_RecoveryReelStops;
        private WayPay m_WayPay;
#if _SIMULATOR

        private List<int> m_WinLineFoundCount;
        //private List<int> m_WinSymbolCount;
        private int m_iTotalWinAmount;
        private int m_iPlaySpinCount;
        private List<SymbolStatistics> m_SymbolStats;
#endif


        public SlotReel(int iSlotNumber, XmlNode node, bool bWayPay)
        {
            m_Random = new Random();
            m_SpinResult = new SpinResult();
            m_TriggerSymbols = new List<TriggerSymbol>();
            m_ScatterSymbols = new List<ScatterSymbol>();
            m_FreeSpinProperties = null;
            m_FreeSpinStopsTickets = null;
            m_bForceSpin = false;
            m_bWayPay = bWayPay;
            if(bWayPay)
            {
                m_WayPay = new WayPay();
            }

#if _SIMULATOR
            m_SymbolStats = new List<SymbolStatistics>();
            m_WinLineFoundCount = new List<int>();
            m_iTotalWinAmount = 0;
            m_iPlaySpinCount = 0;
#endif
        }

        public SlotFeature getSlotFeature(int iFeatureId)
        {
            for (int i = 0; i < m_SlotFeatures.Count; i++)
            {
                if (m_SlotFeatures[i].getFeatureId() == iFeatureId)
                {
                    return m_SlotFeatures[i];
                }
            }
            return null;
        }

        public void SetSlotFeatureBetLevel(int iBetlevel)
        {
            for(int i = 0; i < m_SlotFeatures.Count; i++)
            {
                m_SlotFeatures[i].setBetLevel(iBetlevel);
            }
        }

        public bool LoadXML(XmlNode node)
        {
            XmlNodeList childNodes = node.ChildNodes;
            m_SlotColumns = new List<int[]>();
            m_WildSymbolsID = new List<int>();
            m_WinningLines = new List<int[]>();

            int iNumOfColumns = int.Parse(node.Attributes["col"].Value);
            int iNumOfRows = int.Parse(node.Attributes["row"].Value);
            m_iSlotId = int.Parse(node.Attributes["id"].Value);

            m_iExtraWilds = new List<int>();
            int iNumOfPositions = iNumOfColumns * iNumOfRows;
            for(int i = 0; i < iNumOfPositions; i++)
            {
                m_iExtraWilds.Add(0);
            }

            if (bool.Parse(node.Attributes["freespin"].Value) == true)
            {
                m_FreeSpinProperties = new FreeSpinProperties();
                m_FreeSpinProperties.FreeSpinsTotal =node.Attributes["freespincount"] != null? int.Parse(node.Attributes["freespincount"].Value) : 0;
                m_FreeSpinStopsTickets = new List<string>();
            }

            for (int count = 0; count < iNumOfColumns; count++)
            {
                m_SlotColumns.Add(new int[iNumOfRows]);
                m_iReelStops = new List<int>();//new int[iNumOfColumns];
                for(int i = 0; i < iNumOfColumns; i++)
                {
                    m_iReelStops.Add(0);
                }
            }

            for(int i = 0; i < childNodes.Count; i++)
            {
                switch(childNodes[i].Name)
                {
                    case "REELS":
                        {
                            m_ReelStripList = new List<int[]>();
                            if (!CreateReelStips(childNodes[i]))
                                return false;
                        }
                        break;
                    case "WINNING_LINES":
                        {
                            if(!m_bWayPay)
                            {
                                m_WinningLines = new List<int[]>();
                                if (!CreateWinningLines(childNodes[i]))
                                    return false;

                                m_Winlines = new Winlines();
                                m_Winlines.CreateWinLines(m_WinningLines);
                                m_Winlines.BuildTree();
                            }                           
                        }
                        break;
                    case "PAYTABLE":
                        {
                            m_Paytable = new Paytable(childNodes[i]);
                        }
                        break;
                    case "WILDS":
                        {
                            int[] wildList = StringUtility.StringToIntArray(childNodes[i].Attributes["id"].Value, ' ');
                            for (int wildcount = 0; wildcount < wildList.Length; wildcount++)
                            {
                                m_WildSymbolsID.Add(wildList[wildcount]);
                            }

                        }
                        break;
                    case "TRIGGER":
                        {
                            TriggerSymbol triggerSym = new TriggerSymbol();
                            if(childNodes[i].Attributes["triggertype"] != null)
                            {
                                if (childNodes[i].Attributes["triggertype"].Value == "slotfeature")
                                    triggerSym.triggerType = TriggerSymbol.TYPE.SLOT_FEATURE;
                                else
                                    triggerSym.triggerType = TriggerSymbol.TYPE.BONUS;
                            }
                            else
                                triggerSym.triggerType = TriggerSymbol.TYPE.BONUS;
                            triggerSym.iSymbolId = int.Parse(childNodes[i].Attributes["symbolid"].Value);
                            triggerSym.iNumOfSymbols = int.Parse(childNodes[i].Attributes["numofsymbol"].Value);
                            if (childNodes[i].Attributes["bonusgame"] != null)
                                triggerSym.iBonusId = int.Parse(childNodes[i].Attributes["bonusgame"].Value);
                            if(childNodes[i].Attributes["slotfeature"]!= null)
                                triggerSym.iBonusId = int.Parse(childNodes[i].Attributes["slotfeature"].Value);
                            m_TriggerSymbols.Add(triggerSym);
                        }
                        break;
                    case "SCATTER":
                            {
                                ScatterSymbol scatterSym = new ScatterSymbol();
                                scatterSym.iSymbolId = int.Parse(childNodes[i].Attributes["symbolid"].Value);
                                scatterSym.iNumOfSymbols = int.Parse(childNodes[i].Attributes["numofsymbol"].Value);
                                scatterSym.iCredits = int.Parse(childNodes[i].Attributes["extracredits"].Value);
                                scatterSym.iMultiplier = int.Parse(childNodes[i].Attributes["multiplier"].Value);
                                m_ScatterSymbols.Add(scatterSym);
                            }
                            break;

                }
            }

#if _SIMULATOR
            List<Symbol> AllSymbols = m_Paytable.GetAllSymbols();
            for (int i = 0; i < AllSymbols.Count; i++)
            {
                SymbolStatistics symStat = new SymbolStatistics();
                symStat.numofSymCountList = new Dictionary<int, int>();
                symStat.iSymbolid = AllSymbols[i].SymbolId;
                symStat.symbolCount = 0;
                symStat.iTotalWinAmount = 0;

                Dictionary<int, int> symbolPaytable = AllSymbols[i].getPayoutList();
                foreach (KeyValuePair<int, int> entry in symbolPaytable)
                {
                    symStat.numofSymCountList.Add(entry.Key, 0);
                }
                m_SymbolStats.Add(symStat); 
            }
            for (int i = 0; i < m_WinningLines.Count; i++)
            {
                m_WinLineFoundCount.Add(0);
            }

            for(int i = 0; i < m_ScatterSymbols.Count; i++)
            {
                
                if(StatsContainSymbol(m_ScatterSymbols[i].iSymbolId))
                {
                    SymbolStatistics stat = getSymbolStat(m_ScatterSymbols[i].iSymbolId);
                    stat.numofSymCountList.Add(m_ScatterSymbols[i].iNumOfSymbols, 0);
                }
                else
                {
                    SymbolStatistics symStat = new SymbolStatistics();
                    symStat.numofSymCountList = new Dictionary<int, int>();
                    symStat.iSymbolid = m_ScatterSymbols[i].iSymbolId;
                    symStat.symbolCount = 0;
                    symStat.iTotalWinAmount = 0;
                    symStat.numofSymCountList.Add(m_ScatterSymbols[i].iNumOfSymbols, 0);
                    m_SymbolStats.Add(symStat);
                }

            }
#endif
                return true;
        }

        public bool CreateReelStips(XmlNode node)
        {
            XmlAttribute numberList = node.Attributes["list"];
            XmlNodeList childNodes = node.ChildNodes;
            for(int i = 0; i< childNodes.Count; i++)
            {
                if (childNodes[i].Name == "REELSTRIP")
                {
                    numberList = childNodes[i].Attributes["list"];
                    int[] reelStrip = StringUtility.StringToIntArray(numberList.Value, ' ');
                    m_ReelStripList.Add(new int[reelStrip.Length]);
                    Array.Copy(reelStrip, m_ReelStripList[m_ReelStripList.Count-1], reelStrip.Length);
                }
            }

            if(m_ReelStripList.Count == 0)
            {
                return false;
            }
            return true;
        }

        public Paytable getPayTable()
        {
            return m_Paytable;
        }

        public List<int[]> getWinningLines()
        {
            return m_WinningLines;
        }

        public List<ScatterSymbol> getScatterList()
        {
            return m_ScatterSymbols;
        }

        public List<TriggerSymbol> getTriggerList()
        {
            return m_TriggerSymbols;
        }

        public List<int[]> getReelStrips()
        {
            return m_ReelStripList;
        }

        public List<int[]> getSlotColumns()
        { return m_SlotColumns; }

        public bool CreateWinningLines(XmlNode node)
        {
            XmlAttribute numberList = node.Attributes["list"];
            XmlNodeList childNodes = node.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                if (childNodes[i].Name == "WINNING_LINE")
                {
                    numberList = childNodes[i].Attributes["pos"];
                    int[] reelStrip = StringUtility.StringToIntArray(numberList.Value, ' ');
                    m_WinningLines.Add(new int[reelStrip.Length]);
                    Array.Copy(reelStrip, m_WinningLines[m_WinningLines.Count - 1], reelStrip.Length);
                }
            }

            if (m_WinningLines.Count == 0)
                return false;
            return true;
        }

        public void setReelStopIndex(int index, int iCol)
        {
            /*int offset0;
            int offset2;

            offset0 = index - 1;
            offset2 = index + 1;

            if (offset0 < 0)
            {
                offset0 += m_ReelStripList[iCol].Length - 0;
            }

            if (offset2 >= m_ReelStripList[iCol].Length)
            {
                offset2 -= m_ReelStripList[iCol].Length;
            }
            m_SlotColumns[iCol][0] = m_ReelStripList[iCol][offset0];
            m_SlotColumns[iCol][1] = m_ReelStripList[iCol][index];
            m_SlotColumns[iCol][2] = m_ReelStripList[iCol][offset2];
            */

            int iStartOffset = index;
            int iNumOfRows = m_SlotColumns[iCol].Length;
            
            if(iNumOfRows % 2 == 1)
            {
                iStartOffset -= (iNumOfRows - 1) / 2;
            }
            else
            {
                iStartOffset -= iNumOfRows / 2;
            }

            if(iStartOffset < 0)
            {
                iStartOffset += m_ReelStripList[iCol].Length;
            }

            for(int i = 0; i < iNumOfRows; i++)
            {
                m_SlotColumns[iCol][i] = m_ReelStripList[iCol][iStartOffset];
                iStartOffset++;
                if(iStartOffset >= m_ReelStripList[iCol].Length)
                {
                    iStartOffset -= m_ReelStripList[iCol].Length;
                }
            }
        }

        public void setExtraWilds(List<int> extraWilds)
        {
            for(int i = 0; i < m_iExtraWilds.Count; i++)
            {
                m_iExtraWilds[i] = extraWilds[i];
            }
        }

        public SpinResult PlayTicket(int[] reelStops, int betAmount = 1, bool bFreeSpin = false)
        {
            if(!bFreeSpin)
            {
                try
                {
                    for (int i = 0; i < m_iReelStops.Count; i++)
                    {
                        m_iReelStops[i] = reelStops[i];
                        setReelStopIndex(m_iReelStops[i], i);
                    }
                }
                catch (Exception e)
                {
                    SlotEngine.ThrowError("GAME ENGINE ERROR!");
                    SlotEngine.Log("GAME ENGINE reel stops. SlotReel::PlayTicket() - " + e.Message);
                }


                int getAmount = Evaluate(betAmount, bFreeSpin);
                EvaluateScatterSymbols();
                List<ScatterSymbol> scatterWins = m_SpinResult.getScatterWins();
                if (!m_bWayPay)
                {
                    List<WinLine> winLines = m_Winlines.getWinLines();
                    for (int i = 0; i < scatterWins.Count; i++)
                    {
                        if (scatterWins[i].iCredits != 0)
                        {
                            getAmount += scatterWins[i].iCredits * betAmount;
                            WinLine wLine = new WinLine(-1, scatterWins[i].iSymbolId, scatterWins[i].iNumOfSymbols, scatterWins[i].iCredits * betAmount, null);
                            winLines.Add(wLine);
                        }
                    }
                    m_SpinResult.setResult(getAmount, /*m_Winlines.getWinLines()*/winLines, m_iReelStops);
                }

                EvaluateTriggerLines();
            }
            if (bFreeSpin)
            {
                int[] FSreelStops = StringUtility.StringToIntArray(m_FreeSpinStopsTickets[m_FreeSpinProperties.CurrentFreeSpin],' ');
                try
                {
                    for (int i = 0; i < m_iReelStops.Count; i++)
                    {
                        m_iReelStops[i] = FSreelStops[i];
                        setReelStopIndex(m_iReelStops[i], i);
                    }
                }
                catch(Exception e)
                {
                    SlotEngine.ThrowError("GAME ENGINE ERROR!");
                    SlotEngine.Log("GAME ENGINE FS reel stops. SlotReel::PlayTicket() - " + e.Message);
                }

                int getAmount = Evaluate(betAmount, bFreeSpin);
                EvaluateTriggerLines();
                
                m_FreeSpinProperties.CurrentFreeSpin++;
                m_FreeSpinProperties.WinAmount += m_SpinResult.getWinAmount();
                m_SpinResult.FreeSpinProp = m_FreeSpinProperties;
                if (m_FreeSpinProperties.CurrentFreeSpin == m_FreeSpinProperties.FreeSpinsTotal)
                {
                    m_SpinResult.FreePlay = false;
                }
                else m_SpinResult.FreePlay = true;
            }

            return m_SpinResult;
        }

        public void Recover(int[] reelStops, int iBetAmount)
        {
            m_bRecovery = true;
            m_RecoveryReelStops = new int[reelStops.Length];
            for (int i = 0; i < m_iReelStops.Count; i++)
            {
                m_RecoveryReelStops[i] = reelStops[i];
               // m_iReelStops[i] = reelStops[i];
                //setReelStopIndex(m_iReelStops[i], i);
            }
        }

        public void SetForceSpin(string sForceSpin, bool bSimulator = false)
        {
            if (sForceSpin != "" && !bSimulator)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(sForceSpin);
                XmlNode playNode = doc.SelectSingleNode("PLAY");
                string sReelStops = playNode.Attributes["RS"].Value;

                int[] forcestops = StringUtility.StringToIntArray(sReelStops, ' ');
                SlotEngine.Log("Game Engine: SlotReel::SetForceSpin() - " + "reel stops: " + sReelStops);
                if (forcestops.Length != 0)
                {
                    for (int i = 0; i < m_iReelStops.Count; i++)
                    {
                        m_iReelStops[i] = forcestops[i];
                        setReelStopIndex(m_iReelStops[i], i);
                    }
                    m_bForceSpin = true;
                }
            }
            else if (sForceSpin != "" && bSimulator)
            {
                int[] forcestops = StringUtility.StringToIntArray(sForceSpin, ' ');
                SlotEngine.Log("Game Engine: SlotReel::SetForceSpin() - " + "reel stops: " + sForceSpin);
                if (forcestops.Length != 0)
                {
                    for (int i = 0; i < m_iReelStops.Count; i++)
                    {
                        m_iReelStops[i] = forcestops[i];
                        setReelStopIndex(m_iReelStops[i], i);
                    }
                    m_bForceSpin = true;
                }
            }
        }

        public void ResetFlags()
        {
            m_SpinResult.ResetFlags();
        }

        public SpinResult PlayGame(int betAmount = 1, bool bFreeSpin = false)
        {
            if (m_bRecovery)
            {
                if (m_RecoveryReelStops.Length != 0)
                {
                    for (int i = 0; i < m_iReelStops.Count; i++)
                    {
                        m_iReelStops[i] = m_RecoveryReelStops[i];
                        setReelStopIndex(m_iReelStops[i], i);
                    }
                }
            }
            else if(!m_bForceSpin)
            {
                SlotEngine.Log("Game Engine: SlotReel::PlayGame() - no force spin detected");
                Spin();
            }

            for(int i = 0; i < m_iExtraWilds.Count; i++)
            {
                m_iExtraWilds[i] = 0;
            }

            //int getAmount = m_Winlines.Evaluate(m_SlotColumns, 0, m_Paytable, m_WildSymbolsID, betAmount);
            int getAmount = Evaluate(betAmount, bFreeSpin);//m_Winlines.Evaluate(m_SlotColumns, 0, m_Paytable, m_WildSymbolsID, betAmount);
            EvaluateScatterSymbols();
            List<ScatterSymbol> scatterWins = m_SpinResult.getScatterWins();
            if(!m_bWayPay)
            {
                List<WinLine> winLines = m_Winlines.getWinLines();
                for (int i = 0; i < scatterWins.Count; i++)
                {
                    if (scatterWins[i].iCredits != 0)
                    {
                        getAmount += scatterWins[i].iCredits * betAmount;
                        WinLine wLine = new WinLine(-1, scatterWins[i].iSymbolId, scatterWins[i].iNumOfSymbols, scatterWins[i].iCredits * betAmount, null);
                        winLines.Add(wLine);
                    }
                }
                m_SpinResult.setResult(getAmount, /*m_Winlines.getWinLines()*/winLines, m_iReelStops);
            }
            

            
            EvaluateTriggerLines();

            if(bFreeSpin)
            {
                m_FreeSpinProperties.CurrentFreeSpin++;
                m_FreeSpinProperties.WinAmount += m_SpinResult.getWinAmount();
                m_SpinResult.FreeSpinProp = m_FreeSpinProperties;
                if (m_FreeSpinProperties.CurrentFreeSpin == m_FreeSpinProperties.FreeSpinsTotal)
                {
                    m_SpinResult.FreePlay = false;
                }
                else m_SpinResult.FreePlay = true;
            }
            m_bRecovery = false;
            m_bForceSpin = false;

#if _SIMULATOR
           // CollectStatistics();
            //m_iPlaySpinCount += 1;
#endif
            return m_SpinResult;
        }

        private void EvaluateTriggerLines()
        {
            for (int triggerSymIndex = 0; triggerSymIndex < m_TriggerSymbols.Count; triggerSymIndex++)
            {
                int symCount = 0;
               // bool bSymbolFound = false;

                for (int i = 0; i < m_SlotColumns.Count; i++)
                {
                    //bSymbolFound = false;
                    for(int j = 0; j < m_SlotColumns[i].Length; j++)
                    {
                        if(m_SlotColumns[i][j] == m_TriggerSymbols[triggerSymIndex].iSymbolId)
                        {
                            //bSymbolFound = true;
                            symCount++;
                        }
                    }
                }
                if(symCount == m_TriggerSymbols[triggerSymIndex].iNumOfSymbols)
                {
                    m_SpinResult.ClearTiggerLine();
                    m_SpinResult.AddTriggerLine(m_TriggerSymbols[triggerSymIndex]);
                }
            }
        }

        private void EvaluateScatterSymbols()
        {
            //m_ScatterSymbols.Clear();
            m_SpinResult.ClearScatterWins();
            for( int scatterSymIndex = 0; scatterSymIndex < m_ScatterSymbols.Count; scatterSymIndex++)
            {
                int symCount = 0;
               // bool bSymbolFound = false;

                for (int i = 0; i < m_SlotColumns.Count; i++)
                {
                    //bSymbolFound = false;
                    for (int j = 0; j < m_SlotColumns[i].Length; j++)
                    {
                        if (m_SlotColumns[i][j] == m_ScatterSymbols[scatterSymIndex].iSymbolId)
                        {
                            //bSymbolFound = true;
                            symCount++;
                        }
                    }
                }
                if(symCount == m_ScatterSymbols[scatterSymIndex].iNumOfSymbols)
                {
                    m_SpinResult.AddScatterWin(m_ScatterSymbols[scatterSymIndex]);
                }
            }
        }

        private void Spin()
        {
            int[] randomNumbers = new int[1];
            int generatedNum;
            for(int i = 0; i < m_iReelStops.Count; i++)
            {
                generatedNum = RNG.Random.GetNonUniqueRandomNumbers(0, m_ReelStripList[i].Length - 1, 1, randomNumbers);//m_Random.Next(0, m_ReelStripList[i].Length-1);
                m_iReelStops[i] = randomNumbers[0];
                setReelStopIndex(m_iReelStops[i], i);
            }
            

           // int generatedNum = RNG.Random.GetNonUniqueRandomNumbers(0, m_ReelStripList[i].Length - 1, 1, randomNumbers);
            /*setReelStopIndex(3, 0);
            setReelStopIndex(4, 1);
            setReelStopIndex(7, 2);
            setReelStopIndex(1, 3);
            setReelStopIndex(1, 4); */
        }

        public int Evaluate(int betAmount,/*, List<int[]> slotColumns*/bool bFreeSpin, List<int[]> slotColumns = null)
        {
            if(slotColumns == null)
            {
                slotColumns = m_SlotColumns;
            }
            if(!m_bWayPay)
            {
                int getAmount = m_Winlines.Evaluate(slotColumns, 0, m_Paytable, m_WildSymbolsID, betAmount, m_iExtraWilds);
                EvaluateScatterSymbols();
                List<WinLine> winLines = m_Winlines.getWinLines();
                m_SpinResult.setResult(getAmount, winLines, m_iReelStops);
                return getAmount;
            }
            else
            {
                int iGetAmount = m_WayPay.Evaluate(slotColumns, m_Paytable, m_WildSymbolsID, betAmount, m_iExtraWilds);
                EvaluateScatterSymbols();
                List<WayPayWin> winLines = m_WayPay.getWayPayWins();//m_Winlines.getWinLines();
                m_SpinResult.setResult(iGetAmount, winLines, m_iReelStops);
                return iGetAmount;
            }
        }

        public void InitializeFreeSpin(int iFreeSpinNumber)
        {
            m_FreeSpinProperties.FreeSpinsTotal = iFreeSpinNumber;
            m_FreeSpinProperties.CurrentFreeSpin = 0;
            m_FreeSpinProperties.WinAmount = 0;
        }

        public void RestoreFreeSpinProperties(int iCurrentSpin, int iTotalSpins, int iWinAmount)
        {
            m_FreeSpinProperties.CurrentFreeSpin = iCurrentSpin;
            m_FreeSpinProperties.FreeSpinsTotal = iTotalSpins;
            m_FreeSpinProperties.WinAmount = iWinAmount;
        }

        public void InitFreeStopsTickets(XmlNode node)
        {
            m_FreeSpinStopsTickets.Clear();
            for(int i = 0; i < node.ChildNodes.Count; i++)
            {
                m_FreeSpinStopsTickets.Add(node.ChildNodes[i].Attributes["RS"].Value);
            }
        }

        public FreeSpinProperties getFreeSpinProperties()
        {
            return m_FreeSpinProperties;
        }

        public SpinResult getSpinResult()
        {
            return m_SpinResult;
        }

        public int getSymbolId(int row, int col)
        {
            return m_Winlines.getSymbolId(row, col);
        }

        public void RecalculateWinAmount()
        {
            List<WinLine> wLines = m_Winlines.getWinLines();
            int iTotalWinAmount = 0;
            for(int i = 0; i < wLines.Count; i++)
            {
                iTotalWinAmount += wLines[i].getWinAmount();
            }
            m_SpinResult.setResult(iTotalWinAmount, wLines, m_iReelStops);
        }

        public void AddSlotFeature(SlotFeature feature)
        {
            if(m_SlotFeatures == null)
            {
                m_SlotFeatures = new List<SlotFeature>();
            }
            m_SlotFeatures.Add(feature);
        }

#if _SIMULATOR
        public void CollectStatistics()
        {
            m_iPlaySpinCount += 1;
            if(!m_bWayPay)
            {
                List<WinLine> winningLines = m_Winlines.getWinLines();
                for (int i = 0; i < winningLines.Count; i++)
                {
                    if (winningLines[i].LineID != -1)
                        m_WinLineFoundCount[winningLines[i].LineID] += 1;

                    int symbolStatIndex = getSymbolStatIndex(winningLines[i].SymbolId);
                    SymbolStatistics symStat = m_SymbolStats[symbolStatIndex];
                    symStat.symbolCount += 1;
                    m_SymbolStats[symbolStatIndex].iTotalWinAmount += winningLines[i].getWinAmount();
                    symStat.numofSymCountList[winningLines[i].NumOfSymbols] += 1;

                }
            }
            else
            {
                List<WayPayWin> waypayWins = m_WayPay.getWayPayWins();
                for(int i = 0; i < waypayWins.Count; i++)
                {
                    int symbolStatIndex = getSymbolStatIndex(waypayWins[i].SymbolId);
                    SymbolStatistics symStat = m_SymbolStats[symbolStatIndex];
                    symStat.symbolCount += 1;
                    m_SymbolStats[symbolStatIndex].iTotalWinAmount += waypayWins[i].getWinAmount();
                    symStat.numofSymCountList[waypayWins[i].NumOfSymbols] += 1;
                }
            }
            
            m_iTotalWinAmount += m_SpinResult.getWinAmount();
            
        }

        public int getSymbolStatIndex(int iSymbolId)
        {
            for(int i = 0; i < m_SymbolStats.Count; i++)
            {
                if (iSymbolId == m_SymbolStats[i].iSymbolid)
                    return i;
            }
            return -1;
        }

        public bool StatsContainSymbol(int iSymbolID)
        {
            for (int i = 0; i < m_SymbolStats.Count; i++)
            {
                if (m_SymbolStats[i].iSymbolid == iSymbolID)
                    return true;
            }
            return false;
        }

        public SymbolStatistics getSymbolStat(int iSymbolID)
        {
            for(int i = 0; i < m_SymbolStats.Count; i++)
            {
                if(m_SymbolStats[i].iSymbolid == iSymbolID)
                {
                    return m_SymbolStats[i];
                }
            }
            return null;
        }

        public int getTotalWinAmountStatistics()
        {
            return m_iTotalWinAmount;
        }

        public StringBuilder getStatisticsOutput(int iTotalBetAmount)
        {
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.AppendLine("Slot id: " + m_iSlotId);
            sBuilder.AppendLine("Total win amount: " + m_iTotalWinAmount);
            sBuilder.AppendLine("Total spin count: " + m_iPlaySpinCount);

            decimal percentage = Decimal.Divide(m_iTotalWinAmount, iTotalBetAmount);
            sBuilder.AppendLine("Percent Returns: " + percentage.ToString());
            sBuilder.AppendLine("Slot Reels:");
            string sSlotReel = "";
            for(int i = 0; i < m_ReelStripList.Count; i++)
            {
                //if (i != 0)
                    sSlotReel = "";
                for(int j = 0; j < m_ReelStripList[i].Length; j++)
                {
                    if (j != 0)
                        sSlotReel += " ";
                    sSlotReel += m_ReelStripList[i][j].ToString();
                }
                sBuilder.AppendLine(sSlotReel);
            }
            //sBuilder.AppendLine("Slot Reels:");
            //sBuilder.AppendLine(sSlotReel);

            sBuilder.AppendLine("Winlines");
            sBuilder.AppendLine("__________________________________________________________________________________________________");
            for (int i = 0; i < m_WinLineFoundCount.Count; i++)
            {
                sBuilder.AppendLine("Winline: " + i + "count: " + m_WinLineFoundCount[i]);
            }

            sBuilder.AppendLine("Symbol Counts by Reel");
            sBuilder.AppendLine("SymbolId Reel1 Reel2 Reel3 Reel4 Reel5");
            List<Symbol> allSymbols = m_Paytable.GetAllSymbols();
            for (int i = 0; i < m_WildSymbolsID.Count; i++)
            {
                string sReportSymbol = m_WildSymbolsID[i].ToString() + ": ";
                for (int j = 0; j < m_ReelStripList.Count; j++)
                {
                    int iCount = 0;
                    for (int k = 0; k < m_ReelStripList[j].Length; k++)
                    {
                        if (m_ReelStripList[j][k] == m_WildSymbolsID[i])
                            iCount++;
                    }
                    sReportSymbol += " " + iCount.ToString();
                }
                sBuilder.AppendLine(sReportSymbol);
            }

            for (int i = 0; i < m_TriggerSymbols.Count; i++)
            {
                string sReportSymbol = m_TriggerSymbols[i].iSymbolId.ToString() + ": ";
                for (int j = 0; j < m_ReelStripList.Count; j++)
                {
                    int iCount = 0;
                    for (int k = 0; k < m_ReelStripList[j].Length; k++)
                    {
                        if (m_ReelStripList[j][k] == m_TriggerSymbols[i].iSymbolId)
                            iCount++;
                    }
                    sReportSymbol += " " + iCount.ToString();
                }
                sBuilder.AppendLine(sReportSymbol);
            }

            for (int i = 0; i < allSymbols.Count; i++)
            {
                int iSymbolId = allSymbols[i].SymbolId;
                string sReportSymbol = iSymbolId.ToString() + ": ";
                for (int j = 0; j < m_ReelStripList.Count; j++)
                {
                    int iCount = 0;
                    for (int k = 0; k < m_ReelStripList[j].Length; k++)
                    {
                        if (m_ReelStripList[j][k] == iSymbolId)
                            iCount++;
                    }
                    sReportSymbol += " " + iCount.ToString();
                }
                sBuilder.AppendLine(sReportSymbol);
            }

                sBuilder.AppendLine("Symbols");
            sBuilder.AppendLine("__________________________________________________________________________________________________");
            for (int i = 0; i < m_SymbolStats.Count; i++)
            {
                string symbolWincount = "";
                foreach (KeyValuePair<int, int> entry in m_SymbolStats[i].numofSymCountList)//for(int j = 0; j < m_SymbolStats[i].numofSymCountList.Count; j++)
                {
                    symbolWincount += " numOfSym:" + entry.Key + " wincount:" + entry.Value;
                }
                sBuilder.AppendLine("id: " + m_SymbolStats[i].iSymbolid + symbolWincount + " Total win amount: " + m_SymbolStats[i].iTotalWinAmount);
            }

            if (m_SlotFeatures != null)
            {
                for (int i = 0; i < m_SlotFeatures.Count; i++)
                {
                    sBuilder.AppendLine(m_SlotFeatures[i].getStatisticsOutput(iTotalBetAmount).ToString());
                }
            }

            sBuilder.AppendLine(m_Paytable.getStatisticsOutput().ToString());

                return sBuilder;
        }
#endif
    }
}

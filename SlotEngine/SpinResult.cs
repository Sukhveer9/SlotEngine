using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine
{
    public class SpinResult : EventArgs
    {
        private int m_iWinAmount;
        private int m_iBaseWinAmount;
        private List<WinLine> m_WinLines;
        private List<int> m_ReelStops;
        private bool m_bHaveBonus;
        private bool m_bHasSlotFeature;
        private bool m_bFreePlay;
        private bool m_bIsReady;
        private FreeSpinProperties m_FreeSpinProperties;

        private List<int> m_BonusId; // {previous id, current id, next id}
        private List<int> m_SlotFeatureId; // {previous id, current id, next id}

        private List<SlotReel.TriggerSymbol> m_TriggerLines;
        private List<SlotReel.ScatterSymbol> m_ScatterWins;

        private List<List<int>> m_FSReelStops;
        private List<int> m_BaseReelStops;

        private SlotFeatureResult m_SlotFeatureResult;
        private List<SlotFeatureResult> m_SlotFeatureResultList;

        private List<WayPayWin> m_WayPayWins;

        public SpinResult()
        {
            m_WinLines = new List<WinLine>();
            m_ReelStops = new List<int>();
            m_TriggerLines = new List<SlotReel.TriggerSymbol>();
            m_ScatterWins = new List<SlotReel.ScatterSymbol>();
            m_SlotFeatureResultList = new List<SlotFeatureResult>();
            m_WayPayWins = new List<WayPayWin>();
            m_BonusId = new List<int>();
            m_SlotFeatureId = new List<int>();
            m_BonusId.Add(0); m_BonusId.Add(0); m_BonusId.Add(0);
            m_SlotFeatureId.Add(0); m_SlotFeatureId.Add(0); m_SlotFeatureId.Add(0);
            m_FreeSpinProperties = null;
        }

        public bool FreePlay
        {
            get { return m_bFreePlay; }
            set { m_bFreePlay = value; }
        }

        public bool ReadyForSpin
        {
            get { return m_bIsReady; }
            set { m_bIsReady = value; }
        }

        public int BaseWinAmount
        {
            get { return m_iBaseWinAmount; }
            set { m_iBaseWinAmount = value; }
        }

#if _SIMULATOR
        public List<int> BaseReelStops
        {
            get { return m_BaseReelStops; }
        }

        public List<List<int>> FreeSpinStops
        {
            get { return m_FSReelStops; }
        }
#endif

        public FreeSpinProperties FreeSpinProp
        { get { return m_FreeSpinProperties; } set { m_FreeSpinProperties = value; } }

        public void setResult(int iWinAmount, List<WinLine> winLines, List<int> reelstops)
        {
            m_iWinAmount = iWinAmount;

            //This may have to change in the future, but this is here for Reel Fishing's SuperWild Feature that changes base win amount.
            //m_iBaseWinAmount = iWinAmount;

            m_WinLines = winLines;
            m_ReelStops = reelstops;
        }

        public void setResult(int iWinAmount, List<WayPayWin> wayPayWins, List<int> reelstops)
        {
            m_iWinAmount = iWinAmount;

            //This may have to change in the future, but this is here for Reel Fishing's SuperWild Feature that changes base win amount.
            //m_iBaseWinAmount = iWinAmount;

            m_WayPayWins = wayPayWins;
            m_ReelStops = reelstops;
        }

        public void AddTriggerLine(SlotReel.TriggerSymbol triggerLine)
        {
            m_TriggerLines.Clear();
            m_TriggerLines.Add(triggerLine);
            if (triggerLine.triggerType == SlotReel.TriggerSymbol.TYPE.BONUS)
            {
                m_bHaveBonus = true;
                setBonusId(0, triggerLine.iBonusId, 0);
            }
               
            else if (triggerLine.triggerType == SlotReel.TriggerSymbol.TYPE.SLOT_FEATURE)
                m_bHasSlotFeature = true;

        }

        public void ClearScatterWins()
        {
            m_ScatterWins.Clear();
        }

        public void AddScatterWin(SlotReel.ScatterSymbol scatterWin)
        {
            m_ScatterWins.Add(scatterWin);
        }

        public void AddBaseGameWin(List<int> baseReelStops)
        {
            m_BaseReelStops = baseReelStops;
        }

        public void AddFSReelStops(List<List<int>> FSReelStops)
        {
            m_FSReelStops = FSReelStops;
        }

        public void setBonusId(int iPrev, int iCurr, int iNext)
        {
            m_BonusId[0] = iPrev;
            m_BonusId[1] = iCurr;
            m_BonusId[2] = iNext;
        }

        public void setSlotFeatureId(int iPrev, int iCurr, int iNext)
        {
            m_SlotFeatureId[0] = iPrev;
            m_SlotFeatureId[1] = iCurr;
            m_SlotFeatureId[2] = iNext;
        }

        public void ClearTiggerLine()
        {
            m_TriggerLines.Clear();
            m_bHaveBonus = false;
        }

        public void setBonus(bool bBonus)
        {
            m_bHaveBonus = true;
        }

        public bool hasBonus()
        {
            return m_bHaveBonus;
        }

        public void ResetFlags()
        {
            m_bHaveBonus = false;
            m_bHasSlotFeature = false;
            m_SlotFeatureResultList.Clear();
            m_FreeSpinProperties = null;
            m_iBaseWinAmount = 0;
        }

        public void ResetSlotFeatures()
        {
            m_bHasSlotFeature = false;
            m_SlotFeatureResultList.Clear();
        }

        public bool hasSlotFeature()
        {
            return m_bHasSlotFeature;
        }

        public int getWinAmount()
        {
            return m_iWinAmount;
        }

        public List<WinLine> getWinLines()
        {
            return m_WinLines;
        }

        public List<WayPayWin> getWayPayLines()
        {
            return m_WayPayWins;
        }

        public List<SlotReel.ScatterSymbol> getScatterWins()
        {
            return m_ScatterWins;
        }

        public List<int> getReelStops()
        {
            return m_ReelStops;
        }

        public List<SlotReel.TriggerSymbol> getTriggerLines()
        {
            return m_TriggerLines;
        }

        public List<int> getBonusId()
        {
            return m_BonusId;
        }

        public List<int> getSlotFeatureId()
        {
            return m_SlotFeatureId;
        }

        public void setSlotFeatureResult(SlotFeatureResult result)
        {
            //m_SlotFeatureResult = result;
            bool bAlreadyAdded = false;
            for(int i = 0; i < m_SlotFeatureResultList.Count; i++)
            {
               if(result.getFeatureId() == result.getFeatureId())
               {
                   bAlreadyAdded = true;
                   m_SlotFeatureResultList[i] = result;
                   break;
               }
            }
            if(!bAlreadyAdded)
            {
                m_SlotFeatureResultList.Add(result);
            }
            m_bHasSlotFeature = true;
        }

        public List<SlotFeatureResult> getSlotFeatureResultList()
        {
            return m_SlotFeatureResultList;
        }

        public SlotFeatureResult getSlotFeatureResult()
        {
            return m_SlotFeatureResult;
        }
    }
}

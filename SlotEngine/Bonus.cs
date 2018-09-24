using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;

namespace GameEngine
{
    public class Bonus
    {
        protected int m_BonusId;
        SlotEngine m_Engine;
        protected bool m_bDone;
        protected bool m_bStarted;
        public event EventHandler<EventArgs> BonusResult;
        protected int m_iBetLevel;
        protected int m_iBetCredits;
        protected ePlayType m_ePlayType;

        protected BonusResult m_BonusResult;
        protected Action<string, string> m_RecoveryData;
        protected string m_sMarketType;
        protected bool m_bIgnoreSaveOnce;


        public Bonus(int iBonusID, SlotEngine engineObject, ePlayType type, string sMarketType = "general")
        {
            // m_BonusResult = new BonusResult();
            m_BonusId = iBonusID;
            m_Engine = engineObject;
            m_bDone = true;
            m_bStarted = false;
            m_ePlayType = type;
            m_sMarketType = sMarketType;
        }

        public Bonus(int iBonusID, SlotEngine engineObject, ePlayType type)
        {
            // m_BonusResult = new BonusResult();
            m_BonusId = iBonusID;
            m_Engine = engineObject;
            m_bDone = true;
            m_bStarted = false;
            m_ePlayType = type;
            m_sMarketType = "general";
        }

        public BonusResult SkipBonus(int iBonusWinAmount)
        {
            m_bDone = true;
            m_BonusResult.BonusDone = true;
            m_BonusResult.setBonusWinAmount(iBonusWinAmount);
            m_BonusResult.setBonusId(0, 0, 0);
            return m_BonusResult;
        }

        public BonusResult getBonusResult()
        {
            return m_BonusResult;
        }

        public bool BonusDone
        {
            get { return m_bDone; }
            set { m_bDone = value; }
        }

        public void setBetLevel(int iBetLevel)
        {
            m_iBetLevel = iBetLevel;
        }

        public void setBetCredits(int iBetCredits)
        {
            m_iBetCredits = iBetCredits;
        }

        public bool IsStarted
        { get { return m_bStarted; } set { m_bStarted = false; } }

        public int getBonusid()
        {
            return m_BonusId;
        }

        public void setActionRecoveryData(Action<string, string> recoveryData)
        {
            m_RecoveryData = recoveryData;
        }

        public virtual void RemoveSaveData(string sDataName)
        {

        }

        public void SendBonusResult(object result)
        {
            BonusResult(result, null);
        }

        public virtual void LoadXML(XmlNode node)
        {

        }

        public virtual void SendPick(int iPick)
        {
        }

        public virtual void Initialize()
        {
            m_bDone = false;
            m_bStarted = true;
        }

        public virtual void InitTicket(XmlNode ticketXmlNode)
        {

        }

        public virtual void RecoverBonus(Dictionary<string,string> dataList, int iBetAmount)
        {
            m_iBetLevel = iBetAmount;
        }

        public virtual StringBuilder getStatisticsOutput(int iTotalBetAmount)
        {
            return new StringBuilder();
        }

#if _SIMULATOR
        public virtual int getTotalWinAmountStatistics()
        {
            return 0;
        }
#endif
    }
}

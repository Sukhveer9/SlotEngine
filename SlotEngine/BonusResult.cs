using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class BonusResult : EventArgs
    {
        private bool m_bBonusDone;
        private bool m_bBonusStarted;
        private int m_iBonusID;
        protected int m_iTotalBonusWinAmount;

        private List<int> m_BonusId; // {previous id, current id, next id}

        public BonusResult(int iBonusID)
        {
            m_iBonusID = iBonusID;
            m_BonusId = new List<int>();
            m_BonusId.Add(0); m_BonusId.Add(0); m_BonusId.Add(0);
        }

        public int BonusID
        { get { return m_iBonusID; } }

        public bool BonusDone
        {
            get { return m_bBonusDone; }
            set { m_bBonusDone = value; }
        }

        public bool BonusStarted
        {
            get { return m_bBonusStarted; }
            set { m_bBonusStarted = value; }
        }

        public void setBonusId(int iPrev, int iCurr, int iNext)
        {
            m_BonusId[0] = iPrev;
            m_BonusId[1] = iCurr;
            m_BonusId[2] = iNext;
        }

        public List<int> getBonusId()
        {
            return m_BonusId;
        }

        public void setBonusWinAmount(int iWinAmount)
        {
            m_iTotalBonusWinAmount = iWinAmount;
        }

        public int getBonusWinAmount()
        {
            return m_iTotalBonusWinAmount;
        }
    }


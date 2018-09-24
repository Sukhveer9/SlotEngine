using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine
{
    public class FreeSpinProperties
    {
        private int m_iFreeSpinsTotal;
        private int m_iCurrentFreeSpin;
        private int m_iWinAmount;

        public FreeSpinProperties()
        {
            m_iCurrentFreeSpin = 0;
            m_iFreeSpinsTotal = 0;
            m_iWinAmount = 0;
        }

        public void ResetProperties()
        {
            m_iCurrentFreeSpin = 0;
            //m_iFreeSpinsTotal = 0;
            m_iWinAmount = 0;
        }

        public int FreeSpinsTotal
        {
            get { return m_iFreeSpinsTotal; }
            set { m_iFreeSpinsTotal = value; }
        }

        public int CurrentFreeSpin
        {
            get { return m_iCurrentFreeSpin; }
            set { m_iCurrentFreeSpin = value; }
        }

        public int WinAmount
        {
            get { return m_iWinAmount; }
            set { m_iWinAmount = value; }
        }
    }
}

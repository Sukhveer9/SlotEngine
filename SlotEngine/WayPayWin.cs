using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine
{
    public class WayPayWin
    {
        private int m_iSymbolId;
        private int m_iNumOfSymbols;
        private int m_iNumOfWays;
        private int m_iWinAmount;
        private int m_iMultiplier;
        private int m_iNumOfColumns;
        private List<int> m_SymbolPosList;

        public WayPayWin(int iSymbolId, int iNumOfSymbols, int iNumOfWays, int iWinAmount, int iNumOfCol, List<int> posList)
        {
            m_iSymbolId = iSymbolId;
            m_iNumOfSymbols = iNumOfSymbols;
            m_iWinAmount = iWinAmount;
            m_iMultiplier = 1;
            m_iNumOfWays = iNumOfWays;
            m_iNumOfColumns = iNumOfCol;
            if(m_SymbolPosList == null)
                m_SymbolPosList = new List<int>();
            m_SymbolPosList.Clear();
            for(int i = 0; i < posList.Count; i++)
            {
                m_SymbolPosList.Add(posList[i]);
            }
        }

        public int SymbolId
        { get { return m_iSymbolId; } }
        public int NumOfSymbols
        { get { return m_iNumOfSymbols; } }
        public int Multiplier
        { get { return m_iMultiplier; } }
        public int NumOfColumns
        { get { return m_iNumOfColumns; } }
        public int NumOfWays
        { get { return m_iNumOfWays; } }
        public List<int> SymbolPosList
        {
            get { return m_SymbolPosList; }
        }

        public void applyBetLevel(int iLevel)
        {
            m_iWinAmount *= iLevel;
        }

        public int getWinAmount()
        {
            return m_iWinAmount;
        }
    }
}

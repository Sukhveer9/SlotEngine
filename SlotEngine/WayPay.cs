using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine
{
    public class WayPay
    {
        private Paytable m_PayTable;
        private List<int[]> m_SlotColumns;
        private List<int> m_WildList;
        private List<int> m_iExtraWilds;
        private List<WayPayWin> m_WinLines;

        private List<int> m_AddedSymbols;
        private List<int> m_ColumnCount;

        private List<int> m_SymbolPosList;

        public WayPay()
        {
            m_WinLines = new List<WayPayWin>();
            m_AddedSymbols = new List<int>();
            m_ColumnCount = new List<int>();
            m_SymbolPosList = new List<int>();
            m_iExtraWilds = null;
        }

        public List<WayPayWin> getWayPayWins()
        {
            return m_WinLines;
        }

        public int Evaluate(List<int[]> slotColumns, Paytable pPayTable, List<int> wildList, int iBetAmount, List<int> iExtraWilds = null)
        {
            m_SlotColumns = slotColumns;
            m_PayTable = pPayTable;
            m_WildList = wildList;
            m_iExtraWilds = iExtraWilds;

            m_AddedSymbols.Clear();
            m_WinLines.Clear();
            
            if(m_ColumnCount.Count != slotColumns.Count)
            {
                m_ColumnCount.Clear();
                for(int i = 0; i < slotColumns.Count; i++)
                {
                    m_ColumnCount.Add(0);
                }
            }

            for (int i = 0; i < m_SlotColumns[0].Length; i++)
            {
                if(!m_AddedSymbols.Contains(m_SlotColumns[0][i]))
                {
                    m_AddedSymbols.Add(m_SlotColumns[0][i]);
                }
            }

            for(int i = 0; i < m_AddedSymbols.Count; i++)
            {
                Search(m_AddedSymbols[i]);
            }

            for (int i = 0; i < m_WinLines.Count; i++)
            {
                m_WinLines[i].applyBetLevel(iBetAmount);
            }

            int iTotalWinAmount = 0;
            for (int i = 0; i < m_WinLines.Count; i++)
            {
                iTotalWinAmount += m_WinLines[i].getWinAmount();
            }

            return iTotalWinAmount;

        }

        public bool isWildSymbol(int iSymbolId)
        {
            for (int i = 0; i < m_WildList.Count; i++)
            {
                if (m_WildList[i] == iSymbolId)
                    return true;
            }
            return false;
        }

        public bool isWildSymbol(int row, int col)
        {
            for (int i = 0; i < m_WildList.Count; i++)
            {
                int iSymbolPos = (row * m_SlotColumns.Count) + col;
                if (getSymbolId(col, row) == m_WildList[i])
                {
                    return true;
                }

                else if (m_iExtraWilds != null && m_iExtraWilds[iSymbolPos] == 1)
                {
                    return true;
                }
            }
            return false;
        }

        public int getSymbolId(int col, int row)
        {
            return m_SlotColumns[col][row];
        }

        public void Search(int iSymbolId)
        {
            int iSymCount = 0;
            m_SymbolPosList.Clear();
            for(int i = 0; i < m_ColumnCount.Count; i++)
            {
                m_ColumnCount[i] = 0;
            }
            for(int i = 0; i < m_SlotColumns.Count; i++)
            {
                for (int j = 0; j < m_SlotColumns[i].Length; j++)
                {
                    
                    if(iSymbolId == m_SlotColumns[i][j] || /*isWildSymbol(m_SlotColumns[i][j])*/isWildSymbol(j,i))
                    {
                        m_ColumnCount[i]++;
                        int iPos = 0;
                        iPos = (m_SlotColumns.Count * j) + i;
                        m_SymbolPosList.Add(iPos);
                    }
                }
                if(m_ColumnCount[i] == 0)
                {
                    break;
                }
            }

            int iWays = 1;
            for(int i = 0; i < m_ColumnCount.Count; i++)
            {
                if(m_ColumnCount[i] != 0)
                {
                    iSymCount++;
                    iWays *= m_ColumnCount[i];
                }
                else
                {
                    break;
                }
            }
            if(m_PayTable.getWinAmount(iSymbolId, iSymCount) != 0)
            {
                int iWinAmount = m_PayTable.getWinAmount(iSymbolId, iSymCount);
                iWinAmount *= iWays;
                m_WinLines.Add(new WayPayWin(iSymbolId, iSymCount, iWays, iWinAmount, iSymCount, m_SymbolPosList));
            }

        }
    }
}

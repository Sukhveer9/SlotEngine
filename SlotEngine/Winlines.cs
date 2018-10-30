using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine
{

    public class Winlines
    {
        private List<int[]> m_WinningLines;
        private PaylineNode m_RootNode;
        private Paytable m_PayTable;

        private List<int[]> m_SlotColumns;
        //private List<string> m_PayoutLines;

        private List<WinLine> m_WinLines;
        private List<int> m_WildList;
        private List<int> m_iExtraWilds;

        List<int> m_SearchSymbols;

        public Winlines()
        {
            m_WildList = new List<int>();
            m_iExtraWilds = null;

            m_SearchSymbols = new List<int>();
            m_SearchSymbols.Clear();

          //  m_PayoutLines = new List<string>();
          //  m_PayoutLines.Clear();

            m_WinLines = new List<WinLine>();
            m_WinLines.Clear();
        }

        public List<WinLine> getWinLines()
        {
            return m_WinLines;
        }

        public void CreateWinLines(List<int[]> winLines)
        {
            m_WinningLines = new List<int[]>();
            foreach (int[] winline in winLines)
            {
                m_WinningLines.Add(new int[winline.Length]);
                Array.Copy(winline, m_WinningLines[m_WinningLines.Count - 1], winline.Length);
            }
        }

        public void BuildTree()
        {
            m_RootNode = new PaylineNode();
            m_RootNode.IsRoot = true;
            PaylineNode pointerNode = m_RootNode;

            int counter = 0;
            Queue<int> rowList = new Queue<int>();
            foreach (int[] winline in m_WinningLines)
            {
                rowList.Clear();
                for (int i = 0; i < winline.Length; i++)
                {
                    rowList.Enqueue(winline[i]);
                }
                int index = rowList.Dequeue();
                m_RootNode.AddChild(index, 0, counter, rowList);
                counter++;
            }
        }

        public int Evaluate(List<int[]> slotColumns, int spinNumber, Paytable pPayTable, List<int> wildList, int iBetAmount, List<int> iExtraWilds = null)
        {
            m_SlotColumns = slotColumns;
            m_PayTable = pPayTable;
            m_WildList = wildList;
            m_iExtraWilds = iExtraWilds;
            PaylineNode node = m_RootNode;

            m_WinLines.Clear();
            m_SearchSymbols.Clear();
            //m_PayoutLines.Clear();

            for(int i = 0; i < m_SlotColumns[0].Length; i++)
            {
                if (!m_SearchSymbols.Contains(m_SlotColumns[0][i]))
                {
                    m_SearchSymbols.Add(m_SlotColumns[0][i]);
                }
            }

            for(int i = 0; i < m_SlotColumns[0].Length; i++)
            {
                Search(node.getChild(node.getChildIndex(i)), m_SlotColumns[0][i], false, isWildSymbol(i,0)/*m_SlotColumns[0][0] == 0 ? true : false*/);
            }
            //Search(node.getChild(node.getChildIndex(0)), m_SlotColumns[0][0], false, isWildSymbol(0,0)/*m_SlotColumns[0][0] == 0 ? true : false*/);
            //Search(node.getChild(node.getChildIndex(1)), m_SlotColumns[0][1], false, isWildSymbol(1,0)/*m_SlotColumns[0][1] == 0? true : false*/);
            //Search(node.getChild(node.getChildIndex(2)), m_SlotColumns[0][2], false, isWildSymbol(2,0)/*m_SlotColumns[0][2] == 0 ? true : false*/);


            for (int i = 0; i < m_WinLines.Count; i++)
            {
                m_WinLines[i].applyBetLevel(iBetAmount);
            }
                //VerifyWinLines();

            int iTotalWinAmount = 0;
            for (int i = 0; i < m_WinLines.Count; i++)
            {
                iTotalWinAmount += m_WinLines[i].getWinAmount();
            }

                return iTotalWinAmount;
        }

        public void Search(PaylineNode node, int iSymbolID, bool bEnd, bool bStartWild)
        {
            PaylineNode child = null;
            for(int i = 0; i < node.NumberOfChilds; i++)
            {
                child = node.getChild(i);
                //if (getSymbolId(node.Row, node.col) != 0 && bStartWild)
                if(!isWildSymbol(node.Row, node.col) && bStartWild)
                {
                    iSymbolID = getSymbolId(node.Row, node.col); bStartWild = false;
                }
                //if (getSymbolId(child.Row, child.col) == iSymbolID || getSymbolId(child.Row, child.col) == 0 || bStartWild)
                if (getSymbolId(child.Row, child.col) == iSymbolID || isWildSymbol(child.Row, child.col) || bStartWild)
                {
                    bEnd = false;
                    //wild case
                   // if (getSymbolId(node.Row, node.col) == 0) iSymbolID = getSymbolId(child.Row, child.col);
                    Search(child, iSymbolID, bEnd, bStartWild);
                }
                else
                {
                    bEnd = true;
                }
                if (m_PayTable.IsPaySymbol(iSymbolID)  /*node.col >= 2*/ && !node.IsRoot && bEnd && node.col !=m_SlotColumns.Count)
                {
                    
                    int payline = child.getLastChild().PayLineNum;
                    if(/*(node.col+1) >= m_PayTable.getMinCount(iSymbolID)*/m_PayTable.getWinAmount(iSymbolID, node.col+1) != 0)
                    {
                    m_WinLines.Add(new WinLine(payline, iSymbolID, node.col + 1, m_PayTable.getWinAmount(iSymbolID, node.col + 1), m_WinningLines[payline]));
                  //  m_PayoutLines.Add(" Payline# " + payline + " last column: " + node.col + " row: " + node.Row + "\n");
                    }
                       
                    PaylineNode subChild = child.getLastChild();
                    if (subChild.NumberOfChilds > 0) //if there are other winlines that's a child of the winning winline then add payouts to all the children
                    {
                        for (int j = 0; j < subChild.NumberOfChilds; j++)
                        {
                            PaylineNode eachChild = subChild.getChild(j);
                            int subPayLine = eachChild.PayLineNum;
                            if(subPayLine != payline && m_PayTable.getWinAmount(iSymbolID, node.col+1)!= 0/*(node.col+1)>= m_PayTable.getMinCount(iSymbolID)*/)
                            {
                                m_WinLines.Add(new WinLine(subPayLine, iSymbolID, node.col + 1, m_PayTable.getWinAmount(iSymbolID, node.col + 1), m_WinningLines[subPayLine]));
                          //      m_PayoutLines.Add(" Payline# " + subPayLine + " last column: " + node.col + " row: " + node.Row + "\n");
                            }
                                
                        }
                    }                   
                }
                
            }
            if (node.col == m_SlotColumns.Count-1 && !node.IsRoot /*&& (getSymbolId(node.Row, node.col) == iSymbolID || isWildSymbol(node.Row, node.col))*/ && !bEnd)
            {
                if(isWildSymbol(iSymbolID))
                {
                    iSymbolID = getSymbolId(node.Row, node.col);
                }
                if(m_PayTable.IsPaySymbol(iSymbolID) && m_PayTable.getWinAmount(iSymbolID, node.col + 1) != 0)
                {
                    int payline = node.PayLineNum;
                    m_WinLines.Add(new WinLine(payline, iSymbolID, node.col + 1, m_PayTable.getWinAmount(iSymbolID, node.col + 1), m_WinningLines[payline]));
                  //  m_PayoutLines.Add(" Payline# " + payline + " last column: " + node.col + " row: " + node.Row + "\n");
                }
            }
        }

        public bool isWildSymbol(int row, int col)
        {
            for(int i = 0; i < m_WildList.Count; i++)
            {
                //int iSymbolPos = ((row+1) * (col+1)) -1 ;
                int iSymbolPos = (row * m_SlotColumns.Count) + col;
                /*for(int rowCount = 0; rowCount < row; rowCount++)
                {
                    for(int colCount = 0; colCount < col; colCount++)
                    {
                        iSymbolPos++;
                    }
                }*/
                if(getSymbolId(row, col) == m_WildList[i])
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

        public bool isWildSymbol(int iSymbolId)
        {
            for(int i = 0; i < m_WildList.Count; i++)
            {
                if (m_WildList[i] == iSymbolId)
                    return true;
            }
            return false;
        }

        public int getSymbolId(int row, int col)
        {
            return m_SlotColumns[col][row];
        }

        public List<int[]> getSlotColumns()
        {
            return m_SlotColumns;
        }

        public string getSlotOutput()
        {
            string sOutput = "\n";
            for(int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    sOutput += m_SlotColumns[j][i];
                }
                sOutput += "\n";
            }
            return sOutput;
        }

        public void VerifyWinLines()
        {
            bool bStartWild = false;

            for(int i = 0; i < m_WinLines.Count; i++)
            {
                WinLine line = m_WinLines[i];
                int[] m_WinningLine = m_WinningLines[line.LineID];//new int[] { 0, 0, 0, 0, 0 };//m_WinningLines[line.LineID];
                //if (getSymbolId(m_WinningLine[0], 0) == 0) bStartWild = true;
                if (isWildSymbol(m_WinningLine[0], 0)) bStartWild = true;

                for(int j = 0; j < line.NumOfSymbols; j++)
                {
                    int iSymbol = getSymbolId(m_WinningLine[j], j);
                    if(iSymbol != 0 && bStartWild)
                    {
                        bStartWild = false;
                    }
                    //else if(iSymbol != line.SymbolId && iSymbol != 0)
                    else if (iSymbol != line.SymbolId && !isWildSymbol(iSymbol))
                    {
                        Console.WriteLine(getSlotOutput());
                        Console.WriteLine("ERROR MISMATCH FOR: "+line.toString());
                        break;
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GameEngine
{
    public class Paytable
    {
        private Dictionary<int, Symbol> m_Symbols;

        public Paytable(XmlNode node)
        {
            LoadXML(node);
        }

        public void LoadXML(XmlNode xmlNode)
        {
            XmlNodeList childNodes = xmlNode.ChildNodes;
            Symbol symb = null;
            m_Symbols = new Dictionary<int, Symbol>();
            for(int i = 0; i < childNodes.Count; i++)
            {
                if(childNodes[i].Name == "SYMBOL")
                {
                    symb = new Symbol(childNodes[i]);
                    m_Symbols.Add(symb.SymbolId, symb);
                }
            }
        }

        public int getWinAmount(int iSymbolId, int iNumOfSymbols)
        {
            int iAmount = 0;
            //for(int i = 0; i < m_Symbols.Count; i++)
            //{
            //    if(m_Symbols[i].SymbolId == iSymbolId)
            //    {
            //        iAmount = m_Symbols[i].getWinAmount(iNumOfSymbols);
            //    }
            //}

            if (m_Symbols.ContainsKey(iSymbolId))
            {
                iAmount = m_Symbols[iSymbolId].getWinAmount(iNumOfSymbols);
            }
            return iAmount;
        }

        public bool IsPaySymbol(int iSymbolId)
        {
            //for(int i = 0; i < m_Symbols.Count; i++)
            //{
            //    if (m_Symbols[i].SymbolId == iSymbolId)
            //        return true;
            //}
            //return false;
            return m_Symbols.ContainsKey(iSymbolId);
        }

        public List<Symbol> GetAllSymbols()
        {
            List<Symbol> symbList = new List<Symbol>();
            foreach(KeyValuePair <int, Symbol> kvp in m_Symbols)
            {
                symbList.Add(kvp.Value);
            }
            return symbList;
            //return m_Symbols;
        }

#if _SIMULATOR
        public StringBuilder getStatisticsOutput()
        {
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.AppendLine("PAYTABLE:");
            sBuilder.AppendLine("Symbol  3  4  5");
            Dictionary<int,int> symbolPayout = null;
            //for(int i = 0; i < m_Symbols.Count; i++)
            foreach(KeyValuePair<int, Symbol> symbolElement in m_Symbols)
            {
                int i = symbolElement.Key;
                symbolPayout = m_Symbols[i].getPayoutList();
                //sBuilder.AppendLine(m_Symbols[i].SymbolId + "  " + symbolPayout[3] + "  " + symbolPayout[4] + "  " + symbolPayout[5]);
                string sPayouts = "";
                foreach(KeyValuePair<int, int> payout in symbolPayout)
                {
                    sPayouts += " " + payout.Value;
                }
                sBuilder.AppendLine(sPayouts);
            }

            return sBuilder;
        }
#endif
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GameEngine
{
    public class Symbol
    {
        private int m_iSymbolID;
        private Dictionary<int, int> m_PayoutList;

        public Symbol(XmlNode node)
        {
            LoadXML(node);
        }

        public void LoadXML(XmlNode node)
        {
            XmlAttributeCollection attributes = node.Attributes;
            m_PayoutList = new Dictionary<int, int>();

            m_iSymbolID = int.Parse(attributes["id"].Value);

            string sPayout = attributes["payout"].Value;

            int[] ipayout = StringUtility.StringToIntArray(sPayout, ' ');

            string sCount = attributes["count"].Value;
            int[] iCount = StringUtility.StringToIntArray(sCount, ' ');

            for(int i = 0; i < iCount.Length; i++)
            {
                m_PayoutList.Add(iCount[i], ipayout[i]);
            }
        }

        public int SymbolId
        {
            get { return m_iSymbolID; }
        }

        public int getWinAmount(int iNumOfSymbols)
        {
            if (!m_PayoutList.ContainsKey(iNumOfSymbols))
                return 0;
            return m_PayoutList[iNumOfSymbols];
        }

        public Dictionary<int, int> getPayoutList()
        {
            return m_PayoutList;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine
{
    public class PaylineNode
    {
        private List<PaylineNode> m_ChildNodes;
        private int m_iRowNumber;
        private int m_iLevel;
        private int m_iPaylineNumber;

        private bool m_bIsRoot;

        public PaylineNode()
        {
            m_ChildNodes = new List<PaylineNode>();
            m_iRowNumber = -1;
        }

        public PaylineNode(int iRowNumber, int iLevel, int iPaylineNumber)
        {
            m_bIsRoot = false;
            m_iRowNumber = iRowNumber;
            m_iLevel = iLevel;
            m_iPaylineNumber = iPaylineNumber;
            m_ChildNodes = new List<PaylineNode>();
        }

        public bool IsRoot
        {
            set { m_bIsRoot = value; }
            get { return m_bIsRoot; }
        }

        public int Row
        { get { return m_iRowNumber; } }
        public int col
        {
            get { return m_iLevel; }
        }
        public int PayLineNum
        {
            get { return m_iPaylineNumber; }
        }

        public int NumberOfChilds
        {
            get { return m_ChildNodes.Count; }
        }


        public void AddChild(PaylineNode child)
        {
            m_ChildNodes.Add(child);
        }

        public void AddChild(int iRowNumber, int iLevel, int iPaylineNumber, Queue<int> rowList)
        {
            if(!hasChild(iRowNumber))
            {
                m_ChildNodes.Add(new PaylineNode(iRowNumber, iLevel, iPaylineNumber));
            }
            if(rowList.Count != 0)
            {
                int index = getChildIndex(iRowNumber);
                int nextNum = rowList.Dequeue();
                m_ChildNodes[index].AddChild(nextNum, iLevel + 1, iPaylineNumber, rowList);
            }
        }

        public bool hasChild(int row)
        {
            for(int i = 0; i < m_ChildNodes.Count; i++)
            {
                if(m_ChildNodes[i].Row == row)
                {
                    return true;
                }
            }
            return false;
        }

        public int getChildIndex(int row)
        {
            for (int i = 0; i < m_ChildNodes.Count; i++)
            {
                if (m_ChildNodes[i].Row == row)
                {
                    return i;
                }
            }
            return 0;
        }

        public PaylineNode getChild(int iChildNumber)
        {
            return m_ChildNodes[iChildNumber];
        }

        public PaylineNode getLastChild()
        {
            //PaylineNode lastNode = null;
            if(m_ChildNodes.Count == 1)
            {
                return m_ChildNodes[0].getLastChild();
            }
            
            else 
            {
                return this;
            }
        }
    }
}

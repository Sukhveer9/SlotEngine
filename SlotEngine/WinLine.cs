using System;
using System.Collections.Generic;
using System.Text;


public class WinLine
{
    private int m_iWinlineId;
    private int m_iSymbolId;
    private int m_iNumOfSymbols;
    private int m_iWinAmount;
    private int m_iMultiplier;
    private int[] m_WinLinePositions;

    public WinLine(int iWinlineId, int iSymbolId, int iNumOfSymbols, int iWinAmount, int[] winLinePos)
    {
        m_iWinlineId = iWinlineId;
        m_iSymbolId = iSymbolId;
        m_iNumOfSymbols = iNumOfSymbols;
        m_iWinAmount = iWinAmount;
        m_WinLinePositions = winLinePos;
        m_iMultiplier = 1;
    }

    public int SymbolId
    { get { return m_iSymbolId; } }
    public int LineID
    { get { return m_iWinlineId; } }
    public int NumOfSymbols
    { get { return m_iNumOfSymbols; } }
    public int Multiplier
    { get { return m_iMultiplier; } }

    public int getWinAmount()
    {
        return m_iWinAmount;
    }

    public void applyBetLevel(int iLevel)
    {
        m_iWinAmount *= iLevel;
    }

    public void setMultiplier(int iMultiplier)
    {
        m_iMultiplier = iMultiplier;
        m_iWinAmount *= m_iMultiplier;
    }

    public int[] getWinLinePositions()
    {
        return m_WinLinePositions;
    }

    public string toString()
    {
        return ("winline# " + (m_iWinlineId) + " symbolID: " + m_iSymbolId + " numOfSymb: " + m_iNumOfSymbols + " winAMT: " + m_iWinAmount);
    }
}


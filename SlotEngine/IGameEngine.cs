using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;

namespace GameEngine
{
    public interface IGameEngine
    {
        void PlayGame();
        void PlayGame(int betAmount, string sTicket);
        void PlayGameSimulator(int betAmount = 1);
        List<int[]> getReelStrips(bool bFreeSpin);
        int getBetCredits();
        void SentPick(int iPick);
        void SendPickSimulator(int iPick);
        //List<int> getBetLevels();
       // XmlDocument getSaveData();
        void Recover(Dictionary<string, string> dataList, string sTicket = "", int iBetAmount = 0);
        SpinResult getRecoverSpinResult();
        BonusResult getRecoverBonusResult();
        void setActionRecoveryData(Action<string, string> rData);
        List<Symbol> getPaytable(bool bFreespin = false);
        List<SlotReel.ScatterSymbol> getScatterList(bool bFreespin = false);
        List<SlotReel.TriggerSymbol> getTriggerList(bool bFreespin = false);
        void setThrowErrorAction(Action<string> errorAction);
        void setActionLog(Action<string> logAction);
        bool WayPay();
        string getBaseDefaultReelStops();
#if _SIMULATOR
        StringBuilder getStatisticsOutout(int iTotalBetAmount);
        int getTotalWinAmountStatistics();
#endif
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GameEngine
{
    public class SlotFeature
    {
        protected int m_iFeatureId;
        protected int m_iBetLevel;
        protected string m_sFeatureName;
        protected SlotReel m_SlotReel;
        protected bool m_bDone;
        protected SlotFeatureResult m_SlotFeatureResult;
        public event EventHandler<EventArgs> FeatureResult;

        protected SpinResult m_SpinResult;

        public SlotFeature(int iFeatureId, string sFeatureName)
        {
            m_iFeatureId = iFeatureId;
            m_sFeatureName = sFeatureName;
            m_bDone = true;
        }

        public virtual void LoadXML(XmlNode node)
        { }

        public virtual void Intialize()
        {
        }

                    public virtual StringBuilder getStatisticsOutput(int iTotalBetAmount)
            {
                return new StringBuilder();
            }

        public void setSlotReel(SlotReel pSlotReel)
        {
            m_SlotReel = pSlotReel;
        }

        public void setSpinResult(SpinResult sResult)
        {
            m_SpinResult = sResult;
        }

        public void setBetLevel(int iBetLevel)
        {
            m_iBetLevel = iBetLevel;
        }

        public virtual void Start()
        { }

        public virtual void SendPick(int iPick)
        { }

        public bool isDone()
        {
            return m_bDone;
        }

        public int getFeatureId()
        {
            return m_iFeatureId;
        }

        public void Recover(Dictionary<string,string> dataList)
        { }

        public void SendFeatureResult(object result)
        {
            FeatureResult(result, null);
        }

        public SlotFeatureResult getSlotFeatureResult()
        {
            return m_SlotFeatureResult;
        }
    }
}

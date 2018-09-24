using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SlotFeatureResult
{
    protected bool m_bFeatureDone;
    protected int m_iFeatureId;
    protected bool m_bFeatureStarted;

    public SlotFeatureResult(int iFeatureId)
    {
        m_iFeatureId = iFeatureId;
    }

    public bool FeatureDone
    {
        get { return m_bFeatureDone; }
        set { m_bFeatureDone = value; }
    }

    public bool FeatureStarted
    {
        get { return m_bFeatureStarted; }
        set { m_bFeatureStarted = value; }
    }

    public int getFeatureId()
    {
        return m_iFeatureId;
    }
}

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//  Converted   V3B3_Update2 to V4  (See info here: https://goo.gl/vJoxLk )
namespace NinjaTrader.NinjaScript.Indicators
{

    [Gui.CategoryOrder("Method", 10)]
    [Gui.CategoryOrder("Indicators", 20)]
    [Gui.CategoryOrder("Parameters", 30)]
    [Gui.CategoryOrder("Divergence Visuals", 40)]
    [Gui.CategoryOrder("Alerts", 50)]
    public class D3SpotterV4 : Indicator
    {
        #region Properties 

        public enum D3SpotterV4IndicatorMethod
        {
            CCI,
            CCI_JMA_MASM,
            DO,
            MACD,
            MACDdiff,
            MACDhistOnly,
            MFI,
            Momentum,
            ROC,
            RSI,
            RVI,
            SMI,
            StochasticsD,
            StochasticsK,
            StochasticsFastD,
            StochasticsFastK,
            StochRSI
        }

        public enum D3SpotterV4PriceType
        {
            High_Low,
            Open_Close,
            SMA1,
            SMA2,
            EMA,
            JMA_MASM
        }

        [NinjaScriptProperty]
        [Display(GroupName = "Method", Order = 10, Name = "Indicator Method", Description = "Indicator method to use for divergence test")]
        public NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4IndicatorMethod Method
        {
            get { return method; }
            set { method = value; }
        }
        private NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4IndicatorMethod method = NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4IndicatorMethod.RSI;

        [NinjaScriptProperty]
        [Display(GroupName = "Parameters", Order = 20, Name = "Limit Indicator Difference", Description = "Indicator Difference Limit for divergence")]
        public double IndicatorDiffLimit
        {
            get { return indicatorDiffLimit; }
            set { indicatorDiffLimit = value; }
        }
        private double indicatorDiffLimit = 0.0;

        [NinjaScriptProperty]
        [Display(GroupName = "Parameters", Order = 30, Name = "Limit Price Difference", Description = "Price Difference Limit for divergence")]
        public double PriceDiffLimit
        {
            get { return priceDiffLimit; }
            set { priceDiffLimit = value; }
        }
        private double priceDiffLimit = 0.0;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Parameters", Order = 40, Name = "ScanWidth Lookback Bars", Description = "High/Low Lookback Scan Width")]
        public int ScanWidth
        {
            get { return scanWidth; }
            set { scanWidth = Math.Max(10, value); }
        }
        private int scanWidth = 10; //Lookback period for finding Higher Highs and Lower Lows - WH changed from 30 to 10

        [Range(1, int.MaxValue)] //WH added 05/07/2017 - see Ln106
        [NinjaScriptProperty]
        [Display(GroupName = "Parameters", Order = 50, Name = "Queue Length", Description = "The number of consecutive candidates to look back at")]
        public int QueueLength 
        {
            get { return queueLength; }
            set { queueLength = Math.Max(3, value); }
        }
        private int queueLength = 3;    //WH changed from QueueLength to queueLength so that could include parameter @ Ln1297-1304

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Parameters", Order = 60, Name = "rDiv Line Lookback Bars", Description = "regular divergence line Lookback")] //WH Added			
        public int Rdivlinelookbackperiod  
        {
            get { return rdivlinelookbackperiod; }
            set { rdivlinelookbackperiod = Math.Max(10, value); }
        }
        private int rdivlinelookbackperiod = 30; //24/09/2017 WH added - LookbackPeriod for regular divergence line

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Parameters", Order = 70, Name = "hDiv Line Lookback Bars", Description = "hidden divergence line Lookback")]  //WH Added			
        public int Hdivlinelookbackperiod
        {
            get { return hdivlinelookbackperiod; }
            set { hdivlinelookbackperiod = Math.Max(10, value); }
        }
        private int hdivlinelookbackperiod = 30; //24/09/2017 WH added - LookbackPeriod for hidden divergence line	- set to same as scanWidth if you do not want hidden diverge lines & 2-3x higher to see them	

        [NinjaScriptProperty]
        [Display(GroupName = "Parameters", Order = 80, Name = "Price Type", Description = "Price Type")]
        public NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4PriceType PType
        {
            get { return pType; }
            set { pType = value; }
        }
        private NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4PriceType pType = NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4PriceType.High_Low;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 10, Name = "CCI Period", Description = "Number of bars used for calculations")]
        public int CCI_Period
        {
            get { return cci_Period; }
            set { cci_Period = Math.Max(1, value); }
        }
        private int cci_Period = 14;

        [Range(1, int.MaxValue)]
        [Display(GroupName = "Indicators", Order = 20, Name = "CCI Level 1")]
        public int CCI_Level_1_
        {
            get { return CCI_Level_1; }
            set { CCI_Level_1 = Math.Max(1, value); }
        }
        private int CCI_Level_1 = 100;

        [Range(1, int.MaxValue)]
        [Display(GroupName = "Indicators", Order = 30, Name = "CCI Level 2")]
        public int CCI_Level_2_
        {
            get { return CCI_Level_2; }
            set { CCI_Level_2 = Math.Max(1, value); }
        }
        private int CCI_Level_2 = 200;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 50, Name = "CCI_JAM_MASM Period", Description = "Number of bars used for calculations")]
        public int CCI_JAM_MASM_Period
        {
            get { return cci_jma_masm_Period; }
            set { cci_jma_masm_Period = Math.Max(1, value); }
        }
        private int cci_jma_masm_Period = 7;

        [Display(GroupName = "Parameters", Order = 60, Name = "CCI_JAM_MASM Coefficient")]
        public double CCI_JAM_MASM_Coefficient
        {
            get { return cCI_JAM_MASM_Coefficient; }
            set { cCI_JAM_MASM_Coefficient = Math.Max(0, value); }
        }
        private double cCI_JAM_MASM_Coefficient = 0.015;

        [Range(1, int.MaxValue)]
        [Display(GroupName = "Indicators", Order = 60, Name = "CCI_JAM_MASM Level1")]
        public int JMA_MASM_Level1_
        {
            get { return JMA_MASM_Level1; }
            set { JMA_MASM_Level1 = Math.Max(1, value); }
        }
        private int JMA_MASM_Level1 = 100;

        [Range(1, int.MaxValue)]
        [Display(GroupName = "Indicators", Order = 70, Name = "CCI_JAM_MASM Level2")]
        public int JMA_MASM_Level2_
        {
            get { return JMA_MASM_Level2; }
            set { JMA_MASM_Level2 = Math.Max(1, value); }
        }
        private int JMA_MASM_Level2 = 200;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 100, Name = "Derivative Osc. Period", Description = "Number of bars used for calculations")]
        public int DO_Period
        {
            get { return do_Period; }
            set { do_Period = Math.Max(1, value); }
        }
        private int do_Period = 14;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 110, Name = "Derivative Osc. Smooth1", Description = "Length of smoothing EMA 1")]
        public int DO_Smooth1
        {
            get { return do_Smooth1; }
            set { do_Smooth1 = Math.Max(1, value); }
        }
        private int do_Smooth1 = 5;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 120, Name = "Derivative Osc. Smooth2", Description = "Length of smoothing EMA 2")]
        public int DO_Smooth2
        {
            get { return do_Smooth2; }
            set { do_Smooth2 = Math.Max(1, value); }
        }
        private int do_Smooth2 = 3;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 130, Name = "Derivative Osc. Smooth3", Description = "Length of final smoothing SMA")]
        public int DO_Smooth3
        {
            get { return do_Smooth3; }
            set { do_Smooth3 = Math.Max(1, value); }
        }
        private int do_Smooth3 = 9;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 150, Name = "MACD Fast", Description = "Macd Fast period")]
        public int Macd_Fast
        {
            get { return macd_Fast; }
            set { macd_Fast = Math.Max(1, value); }
        }
        private int macd_Fast = 12;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 160, Name = "MACD Slow", Description = "Macd slow period")]
        public int Macd_Slow
        {
            get { return macd_Slow; }
            set { macd_Slow = Math.Max(1, value); }
        }
        private int macd_Slow = 26;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 170, Name = "MACD Smooth", Description = "Macd smoothing period")]
        public int Macd_Smooth
        {
            get { return macd_Smooth; }
            set { macd_Smooth = Math.Max(1, value); }
        }
        private int macd_Smooth = 9;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 200, Name = "MFI Period", Description = "Number of Bars used for calculations")]
        public int MFI_Period
        {
            get { return mfi_Period; }
            set { mfi_Period = Math.Max(1, value); }
        }
        private int mfi_Period = 14;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 210, Name = "MFI Overbought Level")]
        public int MFI_Overbought_
        {
            get { return MFI_Overbought; }
            set { MFI_Overbought = Math.Max(1, value); }
        }
        private int MFI_Overbought = 80;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 220, Name = "MFI Oversold Level")]
        public int MFI_Oversold_
        {
            get { return MFI_Oversold; }
            set { MFI_Oversold = Math.Max(1, value); }
        }
        private int MFI_Oversold = 20;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 250, Name = "Momentum Period", Description = "Number of Bars used for calculations")]
        public int MOM_Period
        {
            get { return mom_Period; }
            set { mom_Period = Math.Max(1, value); }
        }
        private int mom_Period = 14;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 260, Name = "ROC Period", Description = "Number of Bars used for calculations")]
        public int ROC_Period
        {
            get { return roc_Period; }
            set { roc_Period = Math.Max(1, value); }
        }
        private int roc_Period = 14;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 300, Name = "RSI Period", Description = "Number of Bars used for calculations")]
        public int RSI_Period
        {
            get { return rsi_Period; }
            set { rsi_Period = Math.Max(1, value); }
        }
        private int rsi_Period = 14;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 310, Name = "RSI Smooth", Description = "Number of Bars used for smoothing")]
        public int RSI_Smooth
        {
            get { return rsi_Smooth; }
            set { rsi_Smooth = Math.Max(1, value); }
        }
        private int rsi_Smooth = 3;

        [Range(1, int.MaxValue)]
        [Display(GroupName = "Indicators", Order = 320, Name = "RSI OverSold")]
        public int _RSI_Oversold
        {
            get { return RSI_Oversold; }
            set { RSI_Oversold = value; }
        }
        private int RSI_Oversold = 30;

        [Range(1, int.MaxValue)]
        [Display(GroupName = "Indicators", Order = 330, Name = "RSI OverBought")]
        public int _RSI_Overbought
        {
            get { return RSI_Overbought; }
            set { RSI_Overbought = value; }
        }
        private int RSI_Overbought = 70;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 350, Name = "RVI Period", Description = "Number of Bars used for calculations")]
        public int RVI_Period
        {
            get { return rvi_Period; }
            set { rvi_Period = Math.Max(1, value); }
        }
        private int rvi_Period = 14;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 360, Name = "SMI EMA Period 1", Description = "First EMA smoothing ( R )")]
        public int SMI_EMAPeriod1
        {
            get { return smi_EMAPeriod1; }
            set { smi_EMAPeriod1 = Math.Max(1, value); }
        }
        private int smi_EMAPeriod1 = 25;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 370, Name = "SMI EMA Period 2", Description = "First EMA smoothing ( S )")]
        public int SMI_EMAPeriod2
        {
            get { return smi_EMAPeriod2; }
            set { smi_EMAPeriod2 = Math.Max(1, value); }
        }
        private int smi_EMAPeriod2 = 1;
        /*	
            [Range(1, int.MaxValue)]
            [NinjaScriptProperty]
            [Display(GroupName="Indicators",	Order=380,	Name="SMI Range", Description="Range for momentum calculation ( Q )")]							
            public int SMI_Range
            {
                get { return smi_Range; }
                set { smi_Range = Math.Max(1, value); }
            }
            private int	smi_Range 		= 13;
        */
        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 390, Name = "SMI SMIEMA Period", Description = "SMI EMA smoothing period")]
        public int SMI_SMIEMAPeriod
        {
            get { return smi_SMIEMAPeriod; }
            set { smi_SMIEMAPeriod = Math.Max(1, value); }
        }
        private int smi_SMIEMAPeriod = 25;

        //

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 400, Name = "Stochastics PeriodD", Description = "Numbers of bars used for moving average over D values")]
        public int Stoch_PeriodD
        {
            get { return stoch_PeriodD; }
            set { stoch_PeriodD = Math.Max(1, value); }
        }
        private int stoch_PeriodD = 7;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 410, Name = "Stochastics PeriodK", Description = "Numbers of bars used for calculating K values")]
        public int Stoch_PeriodK
        {
            get { return stoch_PeriodK; }
            set { stoch_PeriodK = Math.Max(1, value); }
        }
        private int stoch_PeriodK = 14;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 420, Name = "Stochastics Smooth", Description = "Numbers of bars for smoothing the slow K values")]
        public int Stoch_Smooth
        {
            get { return stoch_Smooth; }
            set { stoch_Smooth = Math.Max(1, value); }
        }
        private int stoch_Smooth = 3;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 430, Name = "Stochastics Fast PeriodD", Description = "Numbers of bars used for moving average over D values")]
        public int StochFast_PeriodD
        {
            get { return stochfast_PeriodD; }
            set { stochfast_PeriodD = Math.Max(1, value); }
        }
        private int stochfast_PeriodD = 3;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 440, Name = "Stochastics Fast PeriodK", Description = "Numbers of bars for calculating the K values")]
        public int StochFast_PeriodK
        {
            get { return stochfast_PeriodK; }
            set { stochfast_PeriodK = Math.Max(1, value); }
        }
        private int stochfast_PeriodK = 14;

        [Range(1, int.MaxValue)]
        [Display(GroupName = "Indicators", Order = 450, Name = "Stochastics Overbought Level")]
        public int Stoch_Overbought_
        {
            get { return Stoch_Overbought; }
            set { Stoch_Overbought = Math.Max(1, value); }
        }
        private int Stoch_Overbought = 80;

        [Range(1, int.MaxValue)]
        [Display(GroupName = "Indicators", Order = 460, Name = "Stochastics Oversold Level")]
        public int Stoch_Oversold_
        {
            get { return Stoch_Oversold; }
            set { Stoch_Oversold = Math.Max(1, value); }
        }
        private int Stoch_Oversold = 20;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Indicators", Order = 470, Name = "StochRSI Period", Description = "Number of Bars used for calculations")]
        public int StochRSI_Period
        {
            get { return stochrsi_Period; }
            set { stochrsi_Period = Math.Max(1, value); }
        }
        private int stochrsi_Period = 14;

        [Display(GroupName = "Indicators", Order = 480, Name = "StochRSI Oversold")]
        public double StochRSI_Oversold_
        {
            get { return StochRSI_Oversold; }
            set { StochRSI_Oversold = value; }
        }
        private double StochRSI_Oversold = 0.2;

        [Display(GroupName = "Indicators", Order = 490, Name = "StochRSI MedLevel")]
        public double StochRSI_MedLevel_
        {
            get { return StochRSI_MedLevel; }
            set { StochRSI_MedLevel = value; }
        }
        private double StochRSI_MedLevel = 0.5;

        [Display(GroupName = "Indicators", Order = 500, Name = "StochRSI Oversold")]
        public double StochRSI_Overbought_
        {
            get { return StochRSI_Overbought; }
            set { StochRSI_Overbought = value; }
        }
        private double StochRSI_Overbought = 0.8;

        //

        [NinjaScriptProperty]
        [Display(GroupName = "Alerts", Order = 10, Name = "Show Alerts", Description = "Show Alerts")]
        public bool ShowAlerts
        {
            get { return showAlerts; }
            set { showAlerts = value; }
        }
        private bool showAlerts = false;

        [NinjaScriptProperty]
        [Display(GroupName = "Alerts", Order = 20, Name = "High/Low sound", Description = "Sound for detected High/Low")]
        public string MyAlert1
        {
            get { return myAlert1; }
            set { myAlert1 = value; }
        }
        private string myAlert1 = @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert1.wav";

        [NinjaScriptProperty]
        [Display(GroupName = "Alerts", Order = 110, Name = "Divergence sound", Description = "Sound for divergence")]
        public string MyAlert2
        {
            get { return myAlert2; }
            set { myAlert2 = value; }
        }
        private string myAlert2 = @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert2.wav";

        [NinjaScriptProperty]
        [Display(GroupName = "Divergence Visuals", Order = 120, Name = "Use default plots", Description = "Use default plots")]
        public bool UseDefaultPlot
        {
            get { return useDefaultPlot; }
            set { useDefaultPlot = value; }
        }
        private bool useDefaultPlot = true;

        [XmlIgnore]
        [Display(GroupName = "Divergence Visuals", Order = 10, Name = "Color", Description = "Color of hidden divergence line plotted")]        //WH added
        public Brush HiddenDivergenceColor
        {
            get { return hiddenDivergenceColor; }
            set { hiddenDivergenceColor = value; ; }
        }
        [Browsable(false)]
        public string hiddenDivergenceColorSerializable
        {
            get { return Serialize.BrushToString(hiddenDivergenceColor); }
            set { hiddenDivergenceColor = Serialize.StringToBrush(value); }
        }
        private Brush hiddenDivergenceColor = Brushes.DarkBlue;

        [XmlIgnore]
        [Display(GroupName = "Divergence Visuals", Order = 20, Name = "Color", Description = "Color of divergence line plotted")]
        public Brush DivergenceColor
        {
            get { return divergenceColor; }
            set { divergenceColor = value; ; }
        }
        [Browsable(false)]
        public string divergenceColorSerializable
        {
            get { return Serialize.BrushToString(divergenceColor); }
            set { divergenceColor = Serialize.StringToBrush(value); }
        }
        private Brush divergenceColor = Brushes.DarkMagenta;

        [NinjaScriptProperty]
        [Display(GroupName = "Divergence Visuals", Order = 30, Name = "Dash style", Description = "Dashstyle of the divergence line")]
        public DashStyleHelper DivergenceDashStyle
        {
            get { return divergenceDashStyle; }
            set { divergenceDashStyle = value; }
        }
        private DashStyleHelper divergenceDashStyle = DashStyleHelper.Dot;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Divergence Visuals", Order = 40, Name = "Line width", Description = "Divergence Line Width")]
        public int DivergenceLineWidth
        {
            get { return divergenceLineWidth; }
            set { divergenceLineWidth = Math.Max(1, value); }
        }
        private int divergenceLineWidth = 2; //WH changed to 2 from 3

        [XmlIgnore]
        [Display(GroupName = "Divergence Visuals", Order = 50, Name = "Lower Dots")]
        public Brush LowerDotColor
        {
            get { return lowerDotColor; }
            set { lowerDotColor = value; ; }
        }
        [Browsable(false)]
        public string lowerDotColorSerializable
        {
            get { return Serialize.BrushToString(lowerDotColor); }
            set { lowerDotColor = Serialize.StringToBrush(value); }
        }
        private Brush lowerDotColor = Brushes.Cyan;

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(GroupName = "Divergence Visuals", Order = 60, Name = "Marker Distance", Description = "Marker distance (ticks) above/below")]
        public int MarkerDistanceFactor
        {
            get { return markerDistanceFactor; }
            set { markerDistanceFactor = Math.Max(1, value); }
        }
        private int markerDistanceFactor = 1;

        [XmlIgnore]
        [Display(GroupName = "Divergence Visuals", Order = 70, Name = "Upper Dots")]
        public Brush UpperDotColor
        {
            get { return upperDotColor; }
            set { upperDotColor = value; ; }
        }
        [Browsable(false)]
        public string upperDotColorSerializable
        {
            get { return Serialize.BrushToString(upperDotColor); }
            set { upperDotColor = Serialize.StringToBrush(value); }
        }
        private Brush upperDotColor = Brushes.Salmon; //Changed from yellow - WH 10/06/2017

        [Range(1, int.MaxValue)]
        [Display(GroupName = "Divergence Visuals", Order = 10, Name = "dotsize", Description = "default size of wingdings dot")]
        public int _dotsize
        {
            get { return dotsize; }
            set { dotsize = Math.Max(1, value); }
        }
        private int dotsize = 13;

        [Display(GroupName = "Divergence Visuals", Order = 20, Name = "dot type", Description = "(different types are assigned to numbers)")]
        public string Dot
        {
            get { return dot; }
            set { dot = value; }
        }
        private string dot = "l";
        #endregion


        #region program variables & serie-properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IndicPlot0
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IndicPlot1
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IndicPlot2
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IndicPlot3
        {
            get { return Values[3]; }
        }


        // Exposed Properties
        /* 
			
			DivergenceSeriesRegular["LONG"]
			DivergenceSeriesRegular["SHORT"]
				and
			DivergenceSeriesHidden["LONG"]
			DivergenceSeriesHidden["SHORT"]
		*/

        [Browsable(false)]
        [XmlIgnore]
        public Series<int> DivergenceRegular
        {
            get { return divergenceRegular; }
        }
        Series<int> divergenceRegular;

        [Browsable(false)]
        [XmlIgnore]
        public Series<int> DivergenceHidden
        {
            get { return divergenceHidden; }
        }
        Series<int> divergenceHidden;




        private bool initDone = false;
        private ISeries<double> sourceIndicator;
        private int[] HighBarsAgo;
        private int[] LowBarsAgo;
        private int ThisHigh;
        private int ThisLow;
        private int QHLength = 0;
        private int QLLength = 0;
        private int A = 1;
        private int BarsAgo;

        //		Results
        private double foundValue;
        private double foundAvg;
        private double foundDiff;
        private double foundStochD;
        private double foundStochK;

        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Divergence spotter V4 (based on the work of David Anderson of 5/2009)";
                Name = "D3SpotterV4";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = false;  // set to false because of alerts

                // user added
                AddPlot(Brushes.Green, "IndicPlot0");
                AddPlot(Brushes.DarkViolet, "IndicPlot1");
                AddPlot(Brushes.Red, "IndicPlot2");
                AddPlot(new Stroke(Brushes.Navy, 2), PlotStyle.Bar, "IndicPlot3");

                AddLine(Brushes.DarkGray, 0, "IndicLine0");
                AddLine(Brushes.DarkGray, 0, "IndicLine1");
                AddLine(Brushes.DarkGray, 0, "IndicLine2");
                AddLine(Brushes.DarkGray, 0, "IndicLine3");
                AddLine(Brushes.DarkGray, 0, "IndicLine4");
                AddLine(Brushes.DarkGray, 500, "InidicLine5");
                AddLine(Brushes.DarkGray, -500, "IndicLine6");

            }

            else if (State == State.Configure)
            {
                HighBarsAgo = new int[QueueLength];
                LowBarsAgo = new int[QueueLength];

                for (int i = 0; i < QueueLength; i++)
                {
                    HighBarsAgo[i] = 0;
                    LowBarsAgo[i] = 0;
                }
            }
            else if (State == State.DataLoaded)
            {
                divergenceRegular = new Series<int>(this);
                divergenceHidden = new Series<int>(this);

                if (initDone == false)
                {
                    switch (Method)
                    {
                        case D3SpotterV4IndicatorMethod.RSI: InitRSI(); break;
                        case D3SpotterV4IndicatorMethod.MACD: InitMACD(); break;
                        case D3SpotterV4IndicatorMethod.MACDdiff: InitMACD(); break;
                        case D3SpotterV4IndicatorMethod.MACDhistOnly: InitMACDhistOnly(); break;
                        case D3SpotterV4IndicatorMethod.CCI: InitCCI(); break;
                        case D3SpotterV4IndicatorMethod.CCI_JMA_MASM: InitCCI_JMA_MASM(); break;
                        case D3SpotterV4IndicatorMethod.StochasticsD: InitStochastics(); break;
                        case D3SpotterV4IndicatorMethod.StochasticsK: InitStochastics(); break;
                        case D3SpotterV4IndicatorMethod.StochasticsFastD: InitStochastics(); break;
                        case D3SpotterV4IndicatorMethod.StochasticsFastK: InitStochastics(); break;
                        case D3SpotterV4IndicatorMethod.StochRSI: InitStochRSI(); break;
                        case D3SpotterV4IndicatorMethod.MFI: InitMFI(); break;
                        case D3SpotterV4IndicatorMethod.ROC: InitROC(); break;
                        case D3SpotterV4IndicatorMethod.RVI: InitRVI(); break;
                        case D3SpotterV4IndicatorMethod.SMI: InitSMI(); break;
                        case D3SpotterV4IndicatorMethod.DO: InitDO(); break;
                        case D3SpotterV4IndicatorMethod.Momentum: InitMomentum(); break;
                    }
                }
            }
        }

        public override string DisplayName
        {
            get { return " " + this.Name; }
        }


        protected override void OnBarUpdate()
        {
            double PriceDiff, IndicatorDiff;

            if ( CurrentBar < Math.Max( Math.Max(ScanWidth, QueueLength), Math.Max(Rdivlinelookbackperiod, Hdivlinelookbackperiod) ) ) return;
			
            switch (Method)
            {
                case D3SpotterV4IndicatorMethod.RSI: PlotRSI(); break;
                case D3SpotterV4IndicatorMethod.MACD: PlotMACD(); break;
                case D3SpotterV4IndicatorMethod.MACDdiff: PlotMACDdiff(); break;
                case D3SpotterV4IndicatorMethod.MACDhistOnly: PlotMACDhistOnly(); break;
                case D3SpotterV4IndicatorMethod.CCI: PlotCCI(); break;
                case D3SpotterV4IndicatorMethod.CCI_JMA_MASM: PlotCCI_JMA_MASM(); break;
                case D3SpotterV4IndicatorMethod.StochasticsD: PlotStochasticsD(); break;
                case D3SpotterV4IndicatorMethod.StochasticsK: PlotStochasticsK(); break;
                case D3SpotterV4IndicatorMethod.StochasticsFastD: PlotStochasticsFastD(); break;
                case D3SpotterV4IndicatorMethod.StochasticsFastK: PlotStochasticsFastK(); break;
                case D3SpotterV4IndicatorMethod.StochRSI: PlotStochRSI(); break;
                case D3SpotterV4IndicatorMethod.MFI: PlotMFI(); break;
                case D3SpotterV4IndicatorMethod.ROC: PlotROC(); break;
                case D3SpotterV4IndicatorMethod.RVI: PlotRVI(); break;
                case D3SpotterV4IndicatorMethod.SMI: PlotSMI(); break;
                case D3SpotterV4IndicatorMethod.DO: PlotDO(); break;
                case D3SpotterV4IndicatorMethod.Momentum: PlotMomentum(); break;
            }

            switch (PType)
            {
                case D3SpotterV4PriceType.High_Low: ThisHigh = HighestBar(High, ScanWidth); break;
                case D3SpotterV4PriceType.Open_Close: ThisHigh = HighestBar(Close, ScanWidth); break;
                case D3SpotterV4PriceType.SMA1: ThisHigh = HighestBar(SMA(High, 1), ScanWidth); break;
                case D3SpotterV4PriceType.EMA: ThisHigh = HighestBar(EMA(High, 1), ScanWidth); break;
                case D3SpotterV4PriceType.JMA_MASM: ThisHigh = HighestBar(JMA_MASM__D3(High, 1, 0), ScanWidth); break;
            }

            divergenceRegular[0] = 0;
            divergenceHidden[0] = 0;

            // old one: https://pastebin.com/raw/6Nr4H7xC
            if (ThisHigh == A)
            {
                if (ShowAlerts == true)
                {
                    Alert("MyAlrt1" + CurrentBar.ToString(), Priority.High, "D3SpotterV4: High Found", MyAlert1, 10, Brushes.Black, Brushes.Yellow);
                }

                for (int i = QueueLength - 1; i >= 1; i--)
                {
                    HighBarsAgo[i] = HighBarsAgo[i - 1];
                }

                HighBarsAgo[0] = CurrentBar - A;

                //WH used Draw.Text with wingding for dot which can be resized, instead of Draw.Dot, which has larger dot fixed to bar size 
                Draw.Text(this, "Hdot" + CurrentBar.ToString(), true, dot, A, High[A] + (TickSize * MarkerDistanceFactor), 0, UpperDotColor, new SimpleFont("Wingdings", dotsize), TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                DrawOnPricePanel = false;

                Draw.Text(this, "IHdot" + CurrentBar.ToString(), true, dot, A, sourceIndicator[A], 0, UpperDotColor, new SimpleFont("Wingdings", dotsize), TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                DrawOnPricePanel = true;

                if (++QHLength >= 2)
                {
                    for (int i = 0; i < Math.Min(QHLength, QueueLength); i++)
                    {
                        BarsAgo = CurrentBar - HighBarsAgo[i];

                        IndicatorDiff = sourceIndicator[A] - sourceIndicator[BarsAgo];

                        switch (PType)
                        {
                            case D3SpotterV4PriceType.High_Low: PriceDiff = High[A] - High[BarsAgo]; break;
                            case D3SpotterV4PriceType.Open_Close: PriceDiff = Close[A] - Close[BarsAgo]; break;
                            case D3SpotterV4PriceType.SMA1: PriceDiff = SMA(High, 1)[A] - SMA(High, 1)[BarsAgo]; break;
                            default: PriceDiff = High[A] - High[BarsAgo]; break;
                        }

                        //WH adjusted original code 23/09/2017
                        // if (((IndicatorDiff < IndicatorDiffLimit) && (PriceDiff >= PriceDiffLimit)) || ((IndicatorDiff > IndicatorDiffLimit)  && (PriceDiff <= PriceDiffLimit))) 
                        if (IndicatorDiff < IndicatorDiffLimit && (PriceDiff > PriceDiffLimit || PriceDiff.ApproxCompare(PriceDiffLimit) == 0))
                        {
                            //	  < ScanWidth)
                            if ((BarsAgo - A) < rdivlinelookbackperiod)
                            {
                                divergenceRegular[0] = +1;

                                if (ShowAlerts == true)
                                {
                                    Alert("MyAlrt2" + CurrentBar.ToString(), Priority.High, "D3SpotterV4: Divergence Found", MyAlert2, 10, Brushes.Black, Brushes.Red);
                                }

                                Draw.Line(this, "high" + CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, High[BarsAgo] + (TickSize * MarkerDistanceFactor), A,
                                    High[A] + (TickSize * MarkerDistanceFactor), divergenceColor, divergenceDashStyle, divergenceLineWidth);

                                Draw.TriangleDown(this, "MyTriDown" + CurrentBar.ToString(), true, 0, High[0] + (TickSize * MarkerDistanceFactor), Brushes.Red);

                                DrawOnPricePanel = false;
                                Draw.Line(this, "IH" + CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, sourceIndicator[BarsAgo], A, sourceIndicator[A], divergenceColor,
                                    divergenceDashStyle, divergenceLineWidth);

                                DrawOnPricePanel = true;

                            }
                        }
                        else if (IndicatorDiff > IndicatorDiffLimit && (PriceDiff < PriceDiffLimit || PriceDiff.ApproxCompare(PriceDiffLimit) == 0)) //WH Added to give different color to hidden divergence
                        {
                            //	  < ScanWidth)
                            if ((BarsAgo - A) < hdivlinelookbackperiod)
                            {
                                divergenceHidden[0] = +1;
                                if (ShowAlerts == true)
                                {
                                    Alert("MyAlrt2" + CurrentBar.ToString(), Priority.High, "D3SpotterV4: Divergence Found", MyAlert2, 10, Brushes.Black, Brushes.Red);
                                }

                                Draw.Line(this, "high" + CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, High[BarsAgo] + (TickSize * MarkerDistanceFactor), A,
                                    High[A] + (TickSize * MarkerDistanceFactor), hiddenDivergenceColor, divergenceDashStyle, divergenceLineWidth);

                                Draw.TriangleDown(this, "MyTriDown" + CurrentBar.ToString(), true, 0, High[0] + (TickSize * MarkerDistanceFactor), Brushes.Red);

                                DrawOnPricePanel = false;
                                Draw.Line(this, "IH" + CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, sourceIndicator[BarsAgo], A, sourceIndicator[A], hiddenDivergenceColor,
                                    divergenceDashStyle, divergenceLineWidth);

                                DrawOnPricePanel = true;

                            }
                        }
                    }
                }
            }

            switch (PType)
            {
                case D3SpotterV4PriceType.High_Low: ThisLow = LowestBar(Low, ScanWidth); break;
                case D3SpotterV4PriceType.Open_Close: ThisLow = LowestBar(Close, ScanWidth); break;
                case D3SpotterV4PriceType.SMA1: ThisLow = LowestBar(SMA(Low, 1), ScanWidth); break;
                case D3SpotterV4PriceType.EMA: ThisLow = LowestBar(EMA(Low, 1), ScanWidth); break;
                    //				case D3SpotterV4PriceType.JMA_MASM:	ThisLow = LowestBar(JMA_MASM(Low,1,0), ScanWidth);	break;
            }

            if (ThisLow == A)
            {
                for (int i = QueueLength - 1; i >= 1; i--)
                {
                    LowBarsAgo[i] = LowBarsAgo[i - 1];
                }

                LowBarsAgo[0] = CurrentBar - A;

                Draw.Text(this, "Ldot" + CurrentBar.ToString(), true, dot, A, Low[A] - (TickSize * MarkerDistanceFactor), 0, LowerDotColor, new SimpleFont("Wingdings", dotsize), TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                DrawOnPricePanel = false;

                Draw.Text(this, "ILdot" + CurrentBar.ToString(), true, dot, A, sourceIndicator[A], 0, LowerDotColor, new SimpleFont("Wingdings", dotsize), TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                DrawOnPricePanel = true;

                if (ShowAlerts == true)
                {
                    Alert("MyAlrt1" + CurrentBar.ToString(), Priority.High, "D3SpotterV4: Low Found", MyAlert1, 10, Brushes.Black, Brushes.Yellow);
                }

                if (++QLLength >= 2)
                {
                    for (int i = 0; i < Math.Min(QLLength, QueueLength); i++)
                    {
                        BarsAgo = CurrentBar - LowBarsAgo[i];

                        IndicatorDiff = sourceIndicator[A] - sourceIndicator[BarsAgo];
                        switch (PType)
                        {
                            case D3SpotterV4PriceType.High_Low: PriceDiff = Low[A] - Low[BarsAgo]; break;
                            case D3SpotterV4PriceType.Open_Close: PriceDiff = Close[A] - Close[BarsAgo]; break;
                            case D3SpotterV4PriceType.SMA1: PriceDiff = SMA(Close, 1)[A] - SMA(Close, 1)[BarsAgo]; break;
                            default: PriceDiff = Low[A] - Low[BarsAgo]; break;
                        }

                        //WH adjusted original code 23/09/2017
                        // if (((IndicatorDiff > IndicatorDiffLimit) && (PriceDiff <= PriceDiffLimit)) || ((IndicatorDiff < IndicatorDiffLimit)  && (PriceDiff >= PriceDiffLimit))) 
                        if (IndicatorDiff > IndicatorDiffLimit && (PriceDiff < PriceDiffLimit || PriceDiff.ApproxCompare(PriceDiffLimit) == 0))
                        {
                            //	  < ScanWidth)
                            if ((BarsAgo - A) < rdivlinelookbackperiod)
                            {
                                divergenceRegular[0] = -1;

                                Draw.Line(this, "low" + CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, Low[BarsAgo] - (TickSize * MarkerDistanceFactor),
                                    A, Low[A] - (TickSize * MarkerDistanceFactor), DivergenceColor, DivergenceDashStyle, DivergenceLineWidth);

                                Draw.TriangleUp(this, "MyTriUp" + CurrentBar.ToString(), true, 0, Low[0] - (TickSize * MarkerDistanceFactor), Brushes.Green);

                                DrawOnPricePanel = false;

                                Draw.Line(this, "Ilow" + CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, sourceIndicator[BarsAgo],
                                    A, sourceIndicator[A], divergenceColor, divergenceDashStyle, divergenceLineWidth);

                                DrawOnPricePanel = true;

                                if (ShowAlerts == true)
                                {
                                    Alert("MyAlrt2" + CurrentBar.ToString(), Priority.High, "D3SpotterV4: Divergence Found", MyAlert2, 10, Brushes.Black, Brushes.Red);
                                }
                            }
                        }
                        else if (IndicatorDiff < IndicatorDiffLimit && (PriceDiff > PriceDiffLimit || PriceDiff.ApproxCompare(PriceDiffLimit) == 0))
                        {
                            //	  < ScanWidth)
                            if ((BarsAgo - A) < hdivlinelookbackperiod)
                            {
                                divergenceHidden[0] = -1;

                                Draw.Line(this, "low" + CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, Low[BarsAgo] - (TickSize * MarkerDistanceFactor),
                                    A, Low[A] - (TickSize * MarkerDistanceFactor), hiddenDivergenceColor, DivergenceDashStyle, DivergenceLineWidth);

                                Draw.TriangleUp(this, "MyTriUp" + CurrentBar.ToString(), true, 0, Low[0] - (TickSize * MarkerDistanceFactor), Brushes.Green);

                                DrawOnPricePanel = false;

                                Draw.Line(this, "Ilow" + CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, sourceIndicator[BarsAgo],
                                    A, sourceIndicator[A], hiddenDivergenceColor, divergenceDashStyle, divergenceLineWidth);

                                DrawOnPricePanel = true;

                                if (ShowAlerts == true)
                                {
                                    Alert("MyAlrt2" + CurrentBar.ToString(), Priority.High, "D3SpotterV4: Divergence Found", MyAlert2, 10, Brushes.Black, Brushes.Red);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return "";
        }

        #region Configured Indicators


        private void InitRSI()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Green;
                Plots[0].Width = 1;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Plots[1].Brush = Brushes.Orange;
                Plots[1].Width = 1;
                Plots[1].DashStyleHelper = DashStyleHelper.Solid;
                Plots[1].PlotStyle = PlotStyle.Line;

                Lines[0].Brush = Brushes.DarkViolet;
                Lines[0].Value = RSI_Oversold;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;

                Lines[1].Brush = Brushes.YellowGreen;
                Lines[1].Value = RSI_Overbought;
                Lines[1].DashStyleHelper = DashStyleHelper.Solid;
                Lines[1].Width = 1;
            }
        }

        private void PlotRSI()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "RSI", "Using: RSI(" + rsi_Period.ToString() + "," + rsi_Smooth.ToString() + ")", TextPosition.TopLeft);
            DrawOnPricePanel = true;

            sourceIndicator = RSI(Close, rsi_Period, rsi_Smooth);
            foundValue = sourceIndicator[0];
            foundAvg = RSI(Close, rsi_Period, rsi_Smooth).Avg[0];

            IndicPlot0[0] = foundValue;
            IndicPlot1[0] = foundAvg;
        }

        //
        // The RVI (Relative Volatility Index) was developed by Donald Dorsey as a compliment to and a confirmation of momentum based indicators. When used to confirm other signals, only buy when the RVI is over 50 and only sell when the RVI is under 50.
        //
        private void InitRVI()
        {

            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.DarkOrange;
                Plots[0].Width = 1;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Lines[0].Brush = Brushes.LightGray;
                Lines[0].Value = 50;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;

            }
        }

        private void PlotRVI()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "RVI", "Using: RVI(" + rvi_Period.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = RVI(rvi_Period);
            foundValue = sourceIndicator[0];

            IndicPlot0[0] = foundValue;
        }

        //
        // The MACD (Moving Average Convergence/Divergence) is a trend following momentum indicator that shows the relationship between two moving averages of prices.
        //
        private void InitMACD()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Green;
                Plots[0].Width = 1;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Plots[1].Brush = Brushes.DarkViolet;
                Plots[1].Width = 1;
                Plots[1].DashStyleHelper = DashStyleHelper.Solid;
                Plots[1].PlotStyle = PlotStyle.Line;

                Plots[3].Brush = Brushes.Navy;
                Plots[3].Width = 2;
                Plots[3].DashStyleHelper = DashStyleHelper.Solid;
                Plots[3].PlotStyle = PlotStyle.Bar;


                Lines[0].Brush = Brushes.DarkGray;
                Lines[0].Value = 0;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;
            }
        }

        private void InitMACDhistOnly()
        {

            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Navy;
                Plots[0].Width = 2;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Plots[3].Brush = Brushes.Navy;
                Plots[3].Width = 2;
                Plots[3].DashStyleHelper = DashStyleHelper.Solid;
                Plots[3].PlotStyle = PlotStyle.Bar;

                Lines[0].Brush = Brushes.DarkGray;
                Lines[0].Value = 0;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;
            }
        }

        private void PlotMACD()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "MACD", "Using: MACD(" + macd_Fast.ToString() + "," + macd_Slow.ToString() + "," + macd_Smooth.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = MACD(macd_Fast, macd_Slow, macd_Smooth);

            foundValue = sourceIndicator[0];
            foundAvg = MACD(macd_Fast, macd_Slow, macd_Smooth).Avg[0];
            foundDiff = MACD(macd_Fast, macd_Slow, macd_Smooth).Diff[0];

            IndicPlot0[0] = foundValue;
            IndicPlot1[0] = foundAvg;
            IndicPlot3[0] = foundDiff;
        }

        private void PlotMACDdiff()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "MACDdiff", "Using: MACDdiff(" + macd_Fast.ToString() + "," + macd_Slow.ToString() + "," + macd_Smooth.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = MACD(macd_Fast, macd_Slow, macd_Smooth).Diff;

            foundValue = MACD(macd_Fast, macd_Slow, macd_Smooth)[0];
            foundAvg = MACD(macd_Fast, macd_Slow, macd_Smooth).Avg[0];
            foundDiff = MACD(macd_Fast, macd_Slow, macd_Smooth).Diff[0];

            IndicPlot0[0] = foundValue;
            IndicPlot1[0] = foundAvg;
            IndicPlot3[0] = foundDiff;
        }

        private void PlotMACDhistOnly()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "MACDhistogram", "Using: MACDhistogram(" + macd_Fast.ToString() + "," + macd_Slow.ToString() + "," + macd_Smooth.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = MACD(macd_Fast, macd_Slow, macd_Smooth).Diff;
            foundValue = sourceIndicator[0];

            IndicPlot0[0] = foundValue;
            IndicPlot3[0] = foundValue;
        }


        //
        // The Commodity Channel Index (CCI) measures the variation of a security's price from its statistical mean. High values show that prices are unusually high compared to average prices whereas low values indicate that prices are unusually low.
        //
        private void InitCCI()
        {
            if (useDefaultPlot == true)
            {
                Plots[0].Brush = Brushes.Orange;
                Plots[0].Width = 1;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Lines[0].Brush = Brushes.DarkGray;
                Lines[0].Value = 0;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;

                Lines[1].Brush = Brushes.DarkGray;
                Lines[1].Value = CCI_Level_1;
                Lines[1].DashStyleHelper = DashStyleHelper.Solid;
                Lines[1].Width = 1;

                Lines[2].Brush = Brushes.DarkGray;
                Lines[2].Value = CCI_Level_2;
                Lines[2].DashStyleHelper = DashStyleHelper.Solid;
                Lines[2].Width = 1;

                Lines[3].Brush = Brushes.DarkGray;
                Lines[3].Value = -CCI_Level_1;
                Lines[3].DashStyleHelper = DashStyleHelper.Solid;
                Lines[3].Width = 1;

                Lines[4].Brush = Brushes.DarkGray;
                Lines[4].Value = -CCI_Level_2;
                Lines[4].DashStyleHelper = DashStyleHelper.Solid;
                Lines[4].Width = 1;
            }
        }

        private void PlotCCI()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "CCI", "Using: CCI(" + cci_Period.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = CCI(cci_Period);
            foundValue = sourceIndicator[0];

            IndicPlot0[0] = foundValue;
        }

        private void InitCCI_JMA_MASM()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Orange;
                Plots[0].Width = 1;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Lines[0].Brush = Brushes.DarkGray;
                Lines[0].Value = 0;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;

                Lines[1].Brush = Brushes.DarkGray;
                Lines[1].Value = JMA_MASM_Level1;
                Lines[1].DashStyleHelper = DashStyleHelper.Solid;
                Lines[1].Width = 1;

                Lines[2].Brush = Brushes.DarkGray;
                Lines[2].Value = JMA_MASM_Level2;
                Lines[2].DashStyleHelper = DashStyleHelper.Solid;
                Lines[2].Width = 1;

                Lines[3].Brush = Brushes.DarkGray;
                Lines[3].Value = -JMA_MASM_Level1;
                Lines[3].DashStyleHelper = DashStyleHelper.Solid;
                Lines[3].Width = 1;

                Lines[4].Brush = Brushes.DarkGray;
                Lines[4].Value = -JMA_MASM_Level2;
                Lines[4].DashStyleHelper = DashStyleHelper.Solid;
                Lines[4].Width = 1;

            }
        }

        private void PlotCCI_JMA_MASM()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "CCI_LMA_MASM", "Using: CCI_JMA_MASM(" + CCI_JAM_MASM_Period.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = CCI_JMA_MASM__D3(CCI_JAM_MASM_Period, 0, CCI_JAM_MASM_Period, cCI_JAM_MASM_Coefficient);
            foundValue = sourceIndicator[0];

            IndicPlot0[0] = foundValue;
        }

        //
        //	Stochastics
        //
        private void InitStochastics()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Transparent; //WH changed from Green to DodgerBlue to Transparent
                Plots[0].Width = 2; //WH changed from 1 to 2
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Plots[1].Brush = Brushes.Transparent; //WH changed from Orange to Transparent
                Plots[1].Width = 2; //WH changed from 1 to 2
                Plots[1].DashStyleHelper = DashStyleHelper.Solid;
                Plots[1].PlotStyle = PlotStyle.Line;
                //Multicoloured plot - added WH 13/07/2017
                //				if(StochasticsFast(StochFast_PeriodD, StochFast_PeriodK).K[0]-StochasticsFast(StochFast_PeriodD, StochFast_PeriodK).D[0] >
                //					StochasticsFast(StochFast_PeriodD, StochFast_PeriodK).K[1]-StochasticsFast(StochFast_PeriodD, StochFast_PeriodK).D[1])
                //				PlotBrushes[1][0] = Brushes.Lime;
                //				else if(StochasticsFast(StochFast_PeriodD, StochFast_PeriodK).K[0]-StochasticsFast(StochFast_PeriodD, StochFast_PeriodK).D[0] <
                //					StochasticsFast(StochFast_PeriodD, StochFast_PeriodK).K[1]-StochasticsFast(StochFast_PeriodD, StochFast_PeriodK).D[1])
                //				PlotBrushes[1][0] = Brushes.Red;
                //				else
                //				PlotBrushes[1][0] = Brushes.Orange;	

                Lines[0].Brush = Brushes.DarkViolet;
                Lines[0].Value = Stoch_Oversold;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 2; //WH changed from 1 to 2

                Lines[1].Brush = Brushes.YellowGreen;
                Lines[1].Value = Stoch_Overbought;
                Lines[1].DashStyleHelper = DashStyleHelper.Solid;
                Lines[1].Width = 2; //WH changed from 1 to 2

            }
        }

        private void PlotStochasticsD()
        {
            DrawOnPricePanel = false; //WH changed text position from TopLeft to BottomLeft
            Draw.TextFixed(this, "Stochastics", "Using: StochasticsD(" + stoch_PeriodD.ToString() + "," + stoch_PeriodK.ToString() + "," + stoch_Smooth.ToString() + ")", TextPosition.BottomLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = Stochastics(stoch_PeriodD, stoch_PeriodK, stoch_Smooth).D;
            foundStochD = sourceIndicator[0];
            foundStochK = Stochastics(stoch_PeriodD, stoch_PeriodK, stoch_Smooth).K[0];

            IndicPlot0[0] = foundStochD;
            IndicPlot1[0] = foundStochK;
        }

        private void PlotStochasticsK()
        {
            DrawOnPricePanel = false; //WH changed text position from TopLeft to BottomLeft
            Draw.TextFixed(this, "Stochastics", "Using: StochasticsK(" + stoch_PeriodD.ToString() + "," + stoch_PeriodK.ToString() + "," + stoch_Smooth.ToString() + ")", TextPosition.BottomLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = Stochastics(stoch_PeriodD, stoch_PeriodK, stoch_Smooth).K;
            foundStochK = sourceIndicator[0];
            foundStochD = Stochastics(stoch_PeriodD, stoch_PeriodK, stoch_Smooth).D[0];

            IndicPlot0[0] = foundStochD;
            IndicPlot1[0] = foundStochK;
        }

        private void PlotStochasticsFastD()
        {
            DrawOnPricePanel = false; //WH changed text position from TopLeft to BottomLeft
            Draw.TextFixed(this, "Using: StochasticsFast", "StochasticsFastD(" + stochfast_PeriodD.ToString() + "," + stochfast_PeriodK.ToString() + ")", TextPosition.BottomLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = StochasticsFast(stochfast_PeriodD, stochfast_PeriodK).D;
            foundStochD = sourceIndicator[0];
            foundStochK = StochasticsFast(stochfast_PeriodD, stochfast_PeriodK).K[0];

            IndicPlot0[0] = foundStochD;
            IndicPlot1[0] = foundStochK;
        }

        private void PlotStochasticsFastK()
        {
            DrawOnPricePanel = false; //WH changed text position from TopLeft to BottomLeft
            Draw.TextFixed(this, "StochasticsFast", "Using: StochasticsFastK(" + stochfast_PeriodD.ToString() + "," + stochfast_PeriodK.ToString() + ")", TextPosition.BottomLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = StochasticsFast(stochfast_PeriodD, stochfast_PeriodK).K;
            foundStochK = sourceIndicator[0];
            foundStochD = StochasticsFast(stochfast_PeriodD, stochfast_PeriodK).D[0];

            IndicPlot0[0] = foundStochD;
            IndicPlot1[0] = foundStochK;
        }

        //
        //	MFI
        //
        private void InitMFI()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Orange;
                Plots[0].Width = 1;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Lines[0].Brush = Brushes.DarkViolet;
                Lines[0].Value = MFI_Oversold;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;

                Lines[1].Brush = Brushes.YellowGreen;
                Lines[1].Value = MFI_Overbought;
                Lines[1].DashStyleHelper = DashStyleHelper.Solid;
                Lines[1].Width = 1;

            }
        }

        private void PlotMFI()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "MFI", "Using: MFI(" + mfi_Period.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = MFI(mfi_Period);
            foundValue = sourceIndicator[0];

            IndicPlot0[0] = foundValue;
        }

        //
        //	ROC
        //
        private void InitROC()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Blue;
                Plots[0].Width = 1;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Lines[0].Brush = Brushes.DarkGray;
                Lines[0].Value = 0;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;

            }
        }

        private void PlotROC()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "ROC", "Using: ROC(" + roc_Period.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = ROC(roc_Period);
            foundValue = sourceIndicator[0];

            IndicPlot0[0] = foundValue;
        }

        //
        //	StochRSI
        //
        private void InitStochRSI()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Green;
                Plots[0].Width = 1;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Lines[0].Brush = Brushes.Blue;
                Lines[0].Value = StochRSI_MedLevel;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;

                Lines[1].Brush = Brushes.Red;
                Lines[1].Value = StochRSI_Overbought;
                Lines[1].DashStyleHelper = DashStyleHelper.Solid;
                Lines[1].Width = 1;

                Lines[2].Brush = Brushes.Red;
                Lines[2].Value = StochRSI_Oversold;
                Lines[2].DashStyleHelper = DashStyleHelper.Solid;
                Lines[2].Width = 1;

            }
        }

        private void PlotStochRSI()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "Using: StochRSI", "StochRSI(" + stochrsi_Period.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = StochRSI(stochrsi_Period);
            foundValue = sourceIndicator[0];

            IndicPlot0[0] = foundValue;
        }

        //
        //	SMI
        //
        private void InitSMI()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Green;
                Plots[0].Width = 2;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Plots[1].Brush = Brushes.Orange;
                Plots[1].Width = 1;
                Plots[1].DashStyleHelper = DashStyleHelper.Solid;
                Plots[1].PlotStyle = PlotStyle.Line;

                Lines[0].Brush = Brushes.DarkGray;
                Lines[0].Value = 0;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;
            }
        }

        private void PlotSMI()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "SMI", "Using: SMI_D3(" + smi_EMAPeriod1.ToString() + "," + smi_EMAPeriod2.ToString() + ","   //+ smi_Range.ToString()
                + "," + smi_SMIEMAPeriod.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = SMI__D3(smi_EMAPeriod1, smi_EMAPeriod2, smi_SMIEMAPeriod); //smi_Range, 
            foundValue = sourceIndicator[0];
            foundAvg = SMI__D3(smi_EMAPeriod1, smi_EMAPeriod2, smi_SMIEMAPeriod).SMIEMA[0];

            IndicPlot0[0] = foundValue;
            IndicPlot1[0] = foundAvg;
        }

        //
        // "Constance Brown's Derivative Oscillator as pusblished in 'Technical Analysis for the Trading Professional' p. 293")
        //
        private void InitDO()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Black;
                Plots[0].Width = 2;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Plots[1].Brush = Brushes.Blue;
                Plots[1].Width = 2;
                Plots[1].DashStyleHelper = DashStyleHelper.Solid;
                Plots[1].PlotStyle = PlotStyle.Bar;
                Plots[1].Min = 0;

                Plots[2].Brush = Brushes.Red;
                Plots[2].Width = 2;
                Plots[2].DashStyleHelper = DashStyleHelper.Solid;
                Plots[2].PlotStyle = PlotStyle.Bar;
                Plots[2].Max = 0;

                Lines[0].Brush = Brushes.DarkOliveGreen;
                Lines[0].Value = 0;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;
            }
        }

        private void PlotDO()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "DO", "Using: DO(" + do_Period.ToString() + "," + do_Smooth1.ToString() + "," + do_Smooth2.ToString() + "," + do_Smooth3.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = DerivativeOscillator__D3(do_Period, do_Smooth1, do_Smooth2, do_Smooth3);
            foundValue = sourceIndicator[0];

            IndicPlot0[0] = foundValue;
            IndicPlot1[0] = foundValue;
            IndicPlot2[0] = foundValue;
        }


        //
        // NinjaTraders Momentum indicator
        //
        private void InitMomentum()
        {
            if (useDefaultPlot == true)
            {
                Reset_Colors();

                Plots[0].Brush = Brushes.Green;
                Plots[0].Width = 1;
                Plots[0].DashStyleHelper = DashStyleHelper.Solid;
                Plots[0].PlotStyle = PlotStyle.Line;

                Lines[0].Brush = Brushes.DarkViolet;
                Lines[0].Value = 0;
                Lines[0].DashStyleHelper = DashStyleHelper.Solid;
                Lines[0].Width = 1;
            }
        }

        private void PlotMomentum()
        {
            DrawOnPricePanel = false;
            Draw.TextFixed(this, "Momentum", "Using: Momentum(" + mom_Period.ToString() + ")", TextPosition.TopLeft); //, Color.Black, new Font("Arial", 10), Color.Black, Color.Black, 5);
            DrawOnPricePanel = true;

            sourceIndicator = Momentum(Close, mom_Period);
            foundValue = sourceIndicator[0];

            IndicPlot0[0] = foundValue;
        }

        //reset colors
        private void Reset_Colors()
        {
            Plots[0].Brush = Brushes.Transparent;
            Plots[1].Brush = Brushes.Transparent;
            Plots[2].Brush = Brushes.Transparent;
            Plots[3].Brush = Brushes.Transparent;

            Lines[0].Brush = Brushes.Transparent;
            Lines[1].Brush = Brushes.Transparent;
            Lines[2].Brush = Brushes.Transparent;
            Lines[3].Brush = Brushes.Transparent;
            Lines[4].Brush = Brushes.Transparent;
            Lines[5].Brush = Brushes.Transparent;
            Lines[6].Brush = Brushes.Transparent;
        }
        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private D3SpotterV4[] cacheD3SpotterV4;
		public D3SpotterV4 D3SpotterV4(NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4IndicatorMethod method, double indicatorDiffLimit, double priceDiffLimit, int scanWidth, int queueLength, int rdivlinelookbackperiod, int hdivlinelookbackperiod, NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4PriceType pType, int cCI_Period, int cCI_JAM_MASM_Period, int dO_Period, int dO_Smooth1, int dO_Smooth2, int dO_Smooth3, int macd_Fast, int macd_Slow, int macd_Smooth, int mFI_Period, int mFI_Overbought_, int mFI_Oversold_, int mOM_Period, int rOC_Period, int rSI_Period, int rSI_Smooth, int rVI_Period, int sMI_EMAPeriod1, int sMI_EMAPeriod2, int sMI_SMIEMAPeriod, int stoch_PeriodD, int stoch_PeriodK, int stoch_Smooth, int stochFast_PeriodD, int stochFast_PeriodK, int stochRSI_Period, bool showAlerts, string myAlert1, string myAlert2, bool useDefaultPlot, DashStyleHelper divergenceDashStyle, int divergenceLineWidth, int markerDistanceFactor)
		{
			return D3SpotterV4(Input, method, indicatorDiffLimit, priceDiffLimit, scanWidth, queueLength, rdivlinelookbackperiod, hdivlinelookbackperiod, pType, cCI_Period, cCI_JAM_MASM_Period, dO_Period, dO_Smooth1, dO_Smooth2, dO_Smooth3, macd_Fast, macd_Slow, macd_Smooth, mFI_Period, mFI_Overbought_, mFI_Oversold_, mOM_Period, rOC_Period, rSI_Period, rSI_Smooth, rVI_Period, sMI_EMAPeriod1, sMI_EMAPeriod2, sMI_SMIEMAPeriod, stoch_PeriodD, stoch_PeriodK, stoch_Smooth, stochFast_PeriodD, stochFast_PeriodK, stochRSI_Period, showAlerts, myAlert1, myAlert2, useDefaultPlot, divergenceDashStyle, divergenceLineWidth, markerDistanceFactor);
		}

		public D3SpotterV4 D3SpotterV4(ISeries<double> input, NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4IndicatorMethod method, double indicatorDiffLimit, double priceDiffLimit, int scanWidth, int queueLength, int rdivlinelookbackperiod, int hdivlinelookbackperiod, NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4PriceType pType, int cCI_Period, int cCI_JAM_MASM_Period, int dO_Period, int dO_Smooth1, int dO_Smooth2, int dO_Smooth3, int macd_Fast, int macd_Slow, int macd_Smooth, int mFI_Period, int mFI_Overbought_, int mFI_Oversold_, int mOM_Period, int rOC_Period, int rSI_Period, int rSI_Smooth, int rVI_Period, int sMI_EMAPeriod1, int sMI_EMAPeriod2, int sMI_SMIEMAPeriod, int stoch_PeriodD, int stoch_PeriodK, int stoch_Smooth, int stochFast_PeriodD, int stochFast_PeriodK, int stochRSI_Period, bool showAlerts, string myAlert1, string myAlert2, bool useDefaultPlot, DashStyleHelper divergenceDashStyle, int divergenceLineWidth, int markerDistanceFactor)
		{
			if (cacheD3SpotterV4 != null)
				for (int idx = 0; idx < cacheD3SpotterV4.Length; idx++)
					if (cacheD3SpotterV4[idx] != null && cacheD3SpotterV4[idx].Method == method && cacheD3SpotterV4[idx].IndicatorDiffLimit == indicatorDiffLimit && cacheD3SpotterV4[idx].PriceDiffLimit == priceDiffLimit && cacheD3SpotterV4[idx].ScanWidth == scanWidth && cacheD3SpotterV4[idx].QueueLength == queueLength && cacheD3SpotterV4[idx].Rdivlinelookbackperiod == rdivlinelookbackperiod && cacheD3SpotterV4[idx].Hdivlinelookbackperiod == hdivlinelookbackperiod && cacheD3SpotterV4[idx].PType == pType && cacheD3SpotterV4[idx].CCI_Period == cCI_Period && cacheD3SpotterV4[idx].CCI_JAM_MASM_Period == cCI_JAM_MASM_Period && cacheD3SpotterV4[idx].DO_Period == dO_Period && cacheD3SpotterV4[idx].DO_Smooth1 == dO_Smooth1 && cacheD3SpotterV4[idx].DO_Smooth2 == dO_Smooth2 && cacheD3SpotterV4[idx].DO_Smooth3 == dO_Smooth3 && cacheD3SpotterV4[idx].Macd_Fast == macd_Fast && cacheD3SpotterV4[idx].Macd_Slow == macd_Slow && cacheD3SpotterV4[idx].Macd_Smooth == macd_Smooth && cacheD3SpotterV4[idx].MFI_Period == mFI_Period && cacheD3SpotterV4[idx].MFI_Overbought_ == mFI_Overbought_ && cacheD3SpotterV4[idx].MFI_Oversold_ == mFI_Oversold_ && cacheD3SpotterV4[idx].MOM_Period == mOM_Period && cacheD3SpotterV4[idx].ROC_Period == rOC_Period && cacheD3SpotterV4[idx].RSI_Period == rSI_Period && cacheD3SpotterV4[idx].RSI_Smooth == rSI_Smooth && cacheD3SpotterV4[idx].RVI_Period == rVI_Period && cacheD3SpotterV4[idx].SMI_EMAPeriod1 == sMI_EMAPeriod1 && cacheD3SpotterV4[idx].SMI_EMAPeriod2 == sMI_EMAPeriod2 && cacheD3SpotterV4[idx].SMI_SMIEMAPeriod == sMI_SMIEMAPeriod && cacheD3SpotterV4[idx].Stoch_PeriodD == stoch_PeriodD && cacheD3SpotterV4[idx].Stoch_PeriodK == stoch_PeriodK && cacheD3SpotterV4[idx].Stoch_Smooth == stoch_Smooth && cacheD3SpotterV4[idx].StochFast_PeriodD == stochFast_PeriodD && cacheD3SpotterV4[idx].StochFast_PeriodK == stochFast_PeriodK && cacheD3SpotterV4[idx].StochRSI_Period == stochRSI_Period && cacheD3SpotterV4[idx].ShowAlerts == showAlerts && cacheD3SpotterV4[idx].MyAlert1 == myAlert1 && cacheD3SpotterV4[idx].MyAlert2 == myAlert2 && cacheD3SpotterV4[idx].UseDefaultPlot == useDefaultPlot && cacheD3SpotterV4[idx].DivergenceDashStyle == divergenceDashStyle && cacheD3SpotterV4[idx].DivergenceLineWidth == divergenceLineWidth && cacheD3SpotterV4[idx].MarkerDistanceFactor == markerDistanceFactor && cacheD3SpotterV4[idx].EqualsInput(input))
						return cacheD3SpotterV4[idx];
			return CacheIndicator<D3SpotterV4>(new D3SpotterV4(){ Method = method, IndicatorDiffLimit = indicatorDiffLimit, PriceDiffLimit = priceDiffLimit, ScanWidth = scanWidth, QueueLength = queueLength, Rdivlinelookbackperiod = rdivlinelookbackperiod, Hdivlinelookbackperiod = hdivlinelookbackperiod, PType = pType, CCI_Period = cCI_Period, CCI_JAM_MASM_Period = cCI_JAM_MASM_Period, DO_Period = dO_Period, DO_Smooth1 = dO_Smooth1, DO_Smooth2 = dO_Smooth2, DO_Smooth3 = dO_Smooth3, Macd_Fast = macd_Fast, Macd_Slow = macd_Slow, Macd_Smooth = macd_Smooth, MFI_Period = mFI_Period, MFI_Overbought_ = mFI_Overbought_, MFI_Oversold_ = mFI_Oversold_, MOM_Period = mOM_Period, ROC_Period = rOC_Period, RSI_Period = rSI_Period, RSI_Smooth = rSI_Smooth, RVI_Period = rVI_Period, SMI_EMAPeriod1 = sMI_EMAPeriod1, SMI_EMAPeriod2 = sMI_EMAPeriod2, SMI_SMIEMAPeriod = sMI_SMIEMAPeriod, Stoch_PeriodD = stoch_PeriodD, Stoch_PeriodK = stoch_PeriodK, Stoch_Smooth = stoch_Smooth, StochFast_PeriodD = stochFast_PeriodD, StochFast_PeriodK = stochFast_PeriodK, StochRSI_Period = stochRSI_Period, ShowAlerts = showAlerts, MyAlert1 = myAlert1, MyAlert2 = myAlert2, UseDefaultPlot = useDefaultPlot, DivergenceDashStyle = divergenceDashStyle, DivergenceLineWidth = divergenceLineWidth, MarkerDistanceFactor = markerDistanceFactor }, input, ref cacheD3SpotterV4);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.D3SpotterV4 D3SpotterV4(NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4IndicatorMethod method, double indicatorDiffLimit, double priceDiffLimit, int scanWidth, int queueLength, int rdivlinelookbackperiod, int hdivlinelookbackperiod, NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4PriceType pType, int cCI_Period, int cCI_JAM_MASM_Period, int dO_Period, int dO_Smooth1, int dO_Smooth2, int dO_Smooth3, int macd_Fast, int macd_Slow, int macd_Smooth, int mFI_Period, int mFI_Overbought_, int mFI_Oversold_, int mOM_Period, int rOC_Period, int rSI_Period, int rSI_Smooth, int rVI_Period, int sMI_EMAPeriod1, int sMI_EMAPeriod2, int sMI_SMIEMAPeriod, int stoch_PeriodD, int stoch_PeriodK, int stoch_Smooth, int stochFast_PeriodD, int stochFast_PeriodK, int stochRSI_Period, bool showAlerts, string myAlert1, string myAlert2, bool useDefaultPlot, DashStyleHelper divergenceDashStyle, int divergenceLineWidth, int markerDistanceFactor)
		{
			return indicator.D3SpotterV4(Input, method, indicatorDiffLimit, priceDiffLimit, scanWidth, queueLength, rdivlinelookbackperiod, hdivlinelookbackperiod, pType, cCI_Period, cCI_JAM_MASM_Period, dO_Period, dO_Smooth1, dO_Smooth2, dO_Smooth3, macd_Fast, macd_Slow, macd_Smooth, mFI_Period, mFI_Overbought_, mFI_Oversold_, mOM_Period, rOC_Period, rSI_Period, rSI_Smooth, rVI_Period, sMI_EMAPeriod1, sMI_EMAPeriod2, sMI_SMIEMAPeriod, stoch_PeriodD, stoch_PeriodK, stoch_Smooth, stochFast_PeriodD, stochFast_PeriodK, stochRSI_Period, showAlerts, myAlert1, myAlert2, useDefaultPlot, divergenceDashStyle, divergenceLineWidth, markerDistanceFactor);
		}

		public Indicators.D3SpotterV4 D3SpotterV4(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4IndicatorMethod method, double indicatorDiffLimit, double priceDiffLimit, int scanWidth, int queueLength, int rdivlinelookbackperiod, int hdivlinelookbackperiod, NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4PriceType pType, int cCI_Period, int cCI_JAM_MASM_Period, int dO_Period, int dO_Smooth1, int dO_Smooth2, int dO_Smooth3, int macd_Fast, int macd_Slow, int macd_Smooth, int mFI_Period, int mFI_Overbought_, int mFI_Oversold_, int mOM_Period, int rOC_Period, int rSI_Period, int rSI_Smooth, int rVI_Period, int sMI_EMAPeriod1, int sMI_EMAPeriod2, int sMI_SMIEMAPeriod, int stoch_PeriodD, int stoch_PeriodK, int stoch_Smooth, int stochFast_PeriodD, int stochFast_PeriodK, int stochRSI_Period, bool showAlerts, string myAlert1, string myAlert2, bool useDefaultPlot, DashStyleHelper divergenceDashStyle, int divergenceLineWidth, int markerDistanceFactor)
		{
			return indicator.D3SpotterV4(input, method, indicatorDiffLimit, priceDiffLimit, scanWidth, queueLength, rdivlinelookbackperiod, hdivlinelookbackperiod, pType, cCI_Period, cCI_JAM_MASM_Period, dO_Period, dO_Smooth1, dO_Smooth2, dO_Smooth3, macd_Fast, macd_Slow, macd_Smooth, mFI_Period, mFI_Overbought_, mFI_Oversold_, mOM_Period, rOC_Period, rSI_Period, rSI_Smooth, rVI_Period, sMI_EMAPeriod1, sMI_EMAPeriod2, sMI_SMIEMAPeriod, stoch_PeriodD, stoch_PeriodK, stoch_Smooth, stochFast_PeriodD, stochFast_PeriodK, stochRSI_Period, showAlerts, myAlert1, myAlert2, useDefaultPlot, divergenceDashStyle, divergenceLineWidth, markerDistanceFactor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.D3SpotterV4 D3SpotterV4(NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4IndicatorMethod method, double indicatorDiffLimit, double priceDiffLimit, int scanWidth, int queueLength, int rdivlinelookbackperiod, int hdivlinelookbackperiod, NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4PriceType pType, int cCI_Period, int cCI_JAM_MASM_Period, int dO_Period, int dO_Smooth1, int dO_Smooth2, int dO_Smooth3, int macd_Fast, int macd_Slow, int macd_Smooth, int mFI_Period, int mFI_Overbought_, int mFI_Oversold_, int mOM_Period, int rOC_Period, int rSI_Period, int rSI_Smooth, int rVI_Period, int sMI_EMAPeriod1, int sMI_EMAPeriod2, int sMI_SMIEMAPeriod, int stoch_PeriodD, int stoch_PeriodK, int stoch_Smooth, int stochFast_PeriodD, int stochFast_PeriodK, int stochRSI_Period, bool showAlerts, string myAlert1, string myAlert2, bool useDefaultPlot, DashStyleHelper divergenceDashStyle, int divergenceLineWidth, int markerDistanceFactor)
		{
			return indicator.D3SpotterV4(Input, method, indicatorDiffLimit, priceDiffLimit, scanWidth, queueLength, rdivlinelookbackperiod, hdivlinelookbackperiod, pType, cCI_Period, cCI_JAM_MASM_Period, dO_Period, dO_Smooth1, dO_Smooth2, dO_Smooth3, macd_Fast, macd_Slow, macd_Smooth, mFI_Period, mFI_Overbought_, mFI_Oversold_, mOM_Period, rOC_Period, rSI_Period, rSI_Smooth, rVI_Period, sMI_EMAPeriod1, sMI_EMAPeriod2, sMI_SMIEMAPeriod, stoch_PeriodD, stoch_PeriodK, stoch_Smooth, stochFast_PeriodD, stochFast_PeriodK, stochRSI_Period, showAlerts, myAlert1, myAlert2, useDefaultPlot, divergenceDashStyle, divergenceLineWidth, markerDistanceFactor);
		}

		public Indicators.D3SpotterV4 D3SpotterV4(ISeries<double> input , NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4IndicatorMethod method, double indicatorDiffLimit, double priceDiffLimit, int scanWidth, int queueLength, int rdivlinelookbackperiod, int hdivlinelookbackperiod, NinjaTrader.NinjaScript.Indicators.D3SpotterV4.D3SpotterV4PriceType pType, int cCI_Period, int cCI_JAM_MASM_Period, int dO_Period, int dO_Smooth1, int dO_Smooth2, int dO_Smooth3, int macd_Fast, int macd_Slow, int macd_Smooth, int mFI_Period, int mFI_Overbought_, int mFI_Oversold_, int mOM_Period, int rOC_Period, int rSI_Period, int rSI_Smooth, int rVI_Period, int sMI_EMAPeriod1, int sMI_EMAPeriod2, int sMI_SMIEMAPeriod, int stoch_PeriodD, int stoch_PeriodK, int stoch_Smooth, int stochFast_PeriodD, int stochFast_PeriodK, int stochRSI_Period, bool showAlerts, string myAlert1, string myAlert2, bool useDefaultPlot, DashStyleHelper divergenceDashStyle, int divergenceLineWidth, int markerDistanceFactor)
		{
			return indicator.D3SpotterV4(input, method, indicatorDiffLimit, priceDiffLimit, scanWidth, queueLength, rdivlinelookbackperiod, hdivlinelookbackperiod, pType, cCI_Period, cCI_JAM_MASM_Period, dO_Period, dO_Smooth1, dO_Smooth2, dO_Smooth3, macd_Fast, macd_Slow, macd_Smooth, mFI_Period, mFI_Overbought_, mFI_Oversold_, mOM_Period, rOC_Period, rSI_Period, rSI_Smooth, rVI_Period, sMI_EMAPeriod1, sMI_EMAPeriod2, sMI_SMIEMAPeriod, stoch_PeriodD, stoch_PeriodK, stoch_Smooth, stochFast_PeriodD, stochFast_PeriodK, stochRSI_Period, showAlerts, myAlert1, myAlert2, useDefaultPlot, divergenceDashStyle, divergenceLineWidth, markerDistanceFactor);
		}
	}
}

#endregion

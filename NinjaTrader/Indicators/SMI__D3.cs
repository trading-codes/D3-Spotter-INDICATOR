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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	[Description("The Stochastic Momentum Index has two lines that oscillate between -100 to 100 values.")]
	public class SMI__D3 : Indicator
	{
		#region  User Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(GroupName="Parameters",	Order=10,  Name="EMAPeriod1", Description="1st ema smothing period. ( R )")]		
		public int EMAPeriod1
		{
			get { return emaperiod1; }
			set { emaperiod1 = Math.Max(1, value); }
		}
		private int	emaperiod1	= 25;
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(GroupName="Parameters",	Order=20,  Name="EMAPeriod2", Description="2nd ema smoothing period. ( S )")]		
		public int EMAPeriod2
		{
			get { return emaperiod2; }
			set { emaperiod2 = Math.Max(1, value); }
		}
		private int	emaperiod2	= 1;
 
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(GroupName="Parameters",	Order=30,  Name="Range", Description="Range for momentum calculation. ( Q )")]		
		public int _Range  //dont name Range as it hides global function
		{
			get { return range; }
			set { range = Math.Max(1, value); }
		}
		private int	range	= 13;
		 
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(GroupName="Parameters",	Order=40,  Name="SMIEMAPeriod", Description="SMI EMA smoothing period.")]		
		public int SMIEMAPeriod
		{
			get { return smiemaperiod; }
			set { smiemaperiod = Math.Max(1, value); }
		}
		private int	smiemaperiod	= 25;
 
		#endregion
		

		#region program Variables & Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> smi
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SMIEMA
		{
			get { return Values[1]; }
		}
		private Series<double>		sms;
		private Series<double>		hls;
		private Series<double> 		smis;
		
		#endregion
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SMI__D3";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
				AddPlot(Brushes.Green, "SMI");
				Plots[0].Width = 2;
				AddPlot(Brushes.Orange, "SMIEMA");
				AddLine(Brushes.DarkGray, 0, "Zero line");
			}
			else if (State == State.DataLoaded)
			{
				//stochastic momentums
				sms			= new Series<double>(this);
				//high low diffs
				hls			= new Series<double>(this);
				//stochastic momentum indexes
				smis		= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (( CurrentBar < emaperiod2) | ( CurrentBar < emaperiod1)) 
				return;
			
			//Stochastic Momentum = SM {distance of close - midpoint}
		 	sms[0] = (Close[0] - 0.5 * ((MAX(High, range)[0] + MIN(Low, range)[0])));
			
			//High low diffs
			hls[0] = (MAX(High, range)[0] - MIN(Low, range)[0]);

			//Stochastic Momentum Index = SMI
			double denom = 0.5*EMA(EMA(hls,emaperiod1),emaperiod2)[0];
 			smis[0] = (100*(EMA(EMA(sms,emaperiod1),emaperiod2))[0] / (denom ==0 ? 1 : denom  ));
			
			//Set the current SMI line value
			smi[0] = (smis[0]);
			
			//Set the line value for the SMIEMA by taking the EMA of the SMI
			SMIEMA[0]= (EMA(smis, smiemaperiod)[0]);

		}
	}
}



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SMI__D3[] cacheSMI__D3;
		public SMI__D3 SMI__D3(int eMAPeriod1, int eMAPeriod2, int sMIEMAPeriod)
		{
			return SMI__D3(Input, eMAPeriod1, eMAPeriod2, sMIEMAPeriod);
		}

		public SMI__D3 SMI__D3(ISeries<double> input, int eMAPeriod1, int eMAPeriod2, int sMIEMAPeriod)
		{
			if (cacheSMI__D3 != null)
				for (int idx = 0; idx < cacheSMI__D3.Length; idx++)
					if (cacheSMI__D3[idx] != null && cacheSMI__D3[idx].EMAPeriod1 == eMAPeriod1 && cacheSMI__D3[idx].EMAPeriod2 == eMAPeriod2 && cacheSMI__D3[idx].SMIEMAPeriod == sMIEMAPeriod && cacheSMI__D3[idx].EqualsInput(input))
						return cacheSMI__D3[idx];
			return CacheIndicator<SMI__D3>(new SMI__D3(){ EMAPeriod1 = eMAPeriod1, EMAPeriod2 = eMAPeriod2, SMIEMAPeriod = sMIEMAPeriod }, input, ref cacheSMI__D3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SMI__D3 SMI__D3(int eMAPeriod1, int eMAPeriod2, int sMIEMAPeriod)
		{
			return indicator.SMI__D3(Input, eMAPeriod1, eMAPeriod2, sMIEMAPeriod);
		}

		public Indicators.SMI__D3 SMI__D3(ISeries<double> input , int eMAPeriod1, int eMAPeriod2, int sMIEMAPeriod)
		{
			return indicator.SMI__D3(input, eMAPeriod1, eMAPeriod2, sMIEMAPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SMI__D3 SMI__D3(int eMAPeriod1, int eMAPeriod2, int sMIEMAPeriod)
		{
			return indicator.SMI__D3(Input, eMAPeriod1, eMAPeriod2, sMIEMAPeriod);
		}

		public Indicators.SMI__D3 SMI__D3(ISeries<double> input , int eMAPeriod1, int eMAPeriod2, int sMIEMAPeriod)
		{
			return indicator.SMI__D3(input, eMAPeriod1, eMAPeriod2, sMIEMAPeriod);
		}
	}
}

#endregion

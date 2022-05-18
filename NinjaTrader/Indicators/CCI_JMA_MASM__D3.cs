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
	
	/// <summary>
	/// The Commodity Channel Index (CCI) measures the variation of a security's price from its statistical mean. High values show that prices are unusually high compared to average prices whereas low values indicate that prices are unusually low.
	/// </summary>
	public class CCI_JMA_MASM__D3 : Indicator
	{
		#region User Properties
		
		[NinjaScriptProperty]
		[Display(GroupName = "Parameters",	Order = 10, Name = "Period")]
		public int Period
		{
			get { return period; }
			set { period = Math.Max(1, value); }
		}
		private int	period		= 14;
		
		[NinjaScriptProperty]		
		[Display(GroupName = "Parameters",	Order = 20, Name = "Numbers of Phase")]		
		public int Phase
		{
			get { return phase; }
			set { phase = Math.Max(1, value); }
		}
		private int	phase		= 0;
		
		[NinjaScriptProperty]		
		[Display(GroupName = "Parameters",	Order = 30,	Name = "Numbers of smoothing")]		
		public int Lenght
		{
			get { return length; }
			set { length = Math.Max(1, value); }
		}
		private int length		= 14;
			
		
		[NinjaScriptProperty]
		[Display(GroupName = "Parameters",	Order = 60,	Name = "Coefficient")]		
		public double Coefficient
		{
			get { return coefficient; }
			set { coefficient = Math.Max(0, value); }
		}
		private double	coefficient		= 0.015;
		
		[Display(GroupName = "Parameters",	Order = 40,	Name = "Level1")]		
		public int Level1
		{
			get { return level1; }
			set { level1 = Math.Max(1, value); }
		}
		private int	level1		= 100;
				
		[Display(GroupName = "Parameters",	Order = 50,	Name = "Level1")]		
		public int Level2
		{
			get { return level2; }
			set { level2 = Math.Max(1, value); }
		}
		private int	level2		= 200;
		
		#endregion 
		
	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "CCI_JMA_MASM__D3";
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
				AddPlot(Brushes.Orange, "CCI");
				AddLine(Brushes.DarkGray, level2, "Level 2");
				AddLine(Brushes.DarkGray, level1, "Level 1");
				AddLine(Brushes.DarkGray, 0, "Zero line");
				AddLine(Brushes.DarkGray, -level1, "Level -1");
				AddLine(Brushes.DarkGray, -level2, "Level -2");
			}
			else if (State == State.DataLoaded)
			{
			}
			
		}
		
		
		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0){
				Value[0] = 0;
			}
			else
			{
				double JM_val = JMA_MASM__D3(length, phase)[0];
				double mean = 0;
				for (int idx = Math.Min(CurrentBar, Period - 1); idx >= 0; idx--){
					mean += Math.Abs(Typical[idx] - JM_val);
				}
				Value[0] = (Typical[0] - JM_val) / (mean == 0 ? 1 : (coefficient * (mean / Math.Min(Period, CurrentBar + 1))));
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CCI_JMA_MASM__D3[] cacheCCI_JMA_MASM__D3;
		public CCI_JMA_MASM__D3 CCI_JMA_MASM__D3(int period, int phase, int lenght, double coefficient)
		{
			return CCI_JMA_MASM__D3(Input, period, phase, lenght, coefficient);
		}

		public CCI_JMA_MASM__D3 CCI_JMA_MASM__D3(ISeries<double> input, int period, int phase, int lenght, double coefficient)
		{
			if (cacheCCI_JMA_MASM__D3 != null)
				for (int idx = 0; idx < cacheCCI_JMA_MASM__D3.Length; idx++)
					if (cacheCCI_JMA_MASM__D3[idx] != null && cacheCCI_JMA_MASM__D3[idx].Period == period && cacheCCI_JMA_MASM__D3[idx].Phase == phase && cacheCCI_JMA_MASM__D3[idx].Lenght == lenght && cacheCCI_JMA_MASM__D3[idx].Coefficient == coefficient && cacheCCI_JMA_MASM__D3[idx].EqualsInput(input))
						return cacheCCI_JMA_MASM__D3[idx];
			return CacheIndicator<CCI_JMA_MASM__D3>(new CCI_JMA_MASM__D3(){ Period = period, Phase = phase, Lenght = lenght, Coefficient = coefficient }, input, ref cacheCCI_JMA_MASM__D3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CCI_JMA_MASM__D3 CCI_JMA_MASM__D3(int period, int phase, int lenght, double coefficient)
		{
			return indicator.CCI_JMA_MASM__D3(Input, period, phase, lenght, coefficient);
		}

		public Indicators.CCI_JMA_MASM__D3 CCI_JMA_MASM__D3(ISeries<double> input , int period, int phase, int lenght, double coefficient)
		{
			return indicator.CCI_JMA_MASM__D3(input, period, phase, lenght, coefficient);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CCI_JMA_MASM__D3 CCI_JMA_MASM__D3(int period, int phase, int lenght, double coefficient)
		{
			return indicator.CCI_JMA_MASM__D3(Input, period, phase, lenght, coefficient);
		}

		public Indicators.CCI_JMA_MASM__D3 CCI_JMA_MASM__D3(ISeries<double> input , int period, int phase, int lenght, double coefficient)
		{
			return indicator.CCI_JMA_MASM__D3(input, period, phase, lenght, coefficient);
		}
	}
}

#endregion

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
	[Description("Constance Brown's Derivative Oscillator as pusblished in 'Technical Analysis for the Trading Professional' p. 293")]
	public class DerivativeOscillator__D3 : Indicator
	{
		#region User Properties
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Period", Description="Number of bars used in calcuations", Order=10, GroupName="Parameters")]		
		public int Period
		{
			get { return period; }
			set { period = Math.Max(1, value); }
		}
		private int	period	= 14;
		
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Smooth1", Description="Length of smoothing EMA 1", Order=20, GroupName="Parameters")]		
		public int Smooth1
		{
			get { return smooth1; }
			set { smooth1 = Math.Max(1, value); }
		}
		private int	smooth1	= 5;
		
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Smooth2", Description="Length of smoothing EMA 2", Order=30, GroupName="Parameters")]		
		public int Smooth2
		{
			get { return smooth2; }
			set { smooth2 = Math.Max(1, value); }
		}
		private int	smooth2	= 3;
		
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Smooth3", Description="Length of final smoothing SMA", Order=40, GroupName="Parameters")]		
		public int Smooth3
		{
			get { return smooth3; }
			set { smooth3 = Math.Max(1, value); }
		}
		private int	smooth3	= 9;
		
		[XmlIgnore]
		[Display(Name="PositiveColor", Description="Color selected, if Derivative Oscillator is positive.", Order=10, GroupName="PlotColors")]		//WH added
		public Brush _upColor 
		{
			get { return upColor; }
			set { upColor = value; }
		}	
			[Browsable(false)]
			public string upColorSerializable
			{
				get { return Serialize.BrushToString(upColor); }
				set { upColor = Serialize.StringToBrush(value); }
			}
		private Brush upColor		= Brushes.Blue;
		
		[XmlIgnore]
		[Display(Name="NegativeColor", Description="Color selected, if Derivative Oscillator is negative.", Order=20, GroupName="PlotColors")]		//WH added
		public Brush _downColor 
		{
			get { return downColor; }
			set { downColor = value; }
		}	
			[Browsable(false)]
			public string downColorSerializable
			{
				get { return Serialize.BrushToString(downColor); }
				set { downColor = Serialize.StringToBrush(value); }
			}
		private Brush downColor		= Brushes.Red;
		//
		private Brush neutralColor	= Brushes.SlateGray;
			
		#endregion
			
		#region program Variables & Properties
			
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DerivativeOsc
		{
			get { return Values[0]; }
		}
		private EMA average1;
		private SMA average2;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "DerivativeOscillator__D3";
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
				// PriceTypeSupported	= true;
				// VerticalGridLines   = false;
			}
			else if (State == State.Configure)
			{ 
				AddPlot(Brushes.Transparent, "DerivativeOsc");
				AddLine(Brushes.DarkOliveGreen, 0, "Zero");
				Plots[0].Width = 2;
				Plots[0].PlotStyle = PlotStyle.Bar;
			}
			else if (State == State.DataLoaded)
			{ 
				average1 = EMA(EMA(RSI(Input,period,3), smooth1), smooth2);
				average2 = SMA(average1, smooth3);
			}
		}
			
		protected override void OnBarUpdate()
		{
			if (CurrentBar <=1) return;
			
			DerivativeOsc[0] = (average1[0]-average2[0]);
			
			if (DerivativeOsc[0] > 0)			PlotBrushes[0][0] = upColor;
			else if (DerivativeOsc[0] < 0)		PlotBrushes[0][0] = downColor;
			else								PlotBrushes[0][0] = neutralColor;
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DerivativeOscillator__D3[] cacheDerivativeOscillator__D3;
		public DerivativeOscillator__D3 DerivativeOscillator__D3(int period, int smooth1, int smooth2, int smooth3)
		{
			return DerivativeOscillator__D3(Input, period, smooth1, smooth2, smooth3);
		}

		public DerivativeOscillator__D3 DerivativeOscillator__D3(ISeries<double> input, int period, int smooth1, int smooth2, int smooth3)
		{
			if (cacheDerivativeOscillator__D3 != null)
				for (int idx = 0; idx < cacheDerivativeOscillator__D3.Length; idx++)
					if (cacheDerivativeOscillator__D3[idx] != null && cacheDerivativeOscillator__D3[idx].Period == period && cacheDerivativeOscillator__D3[idx].Smooth1 == smooth1 && cacheDerivativeOscillator__D3[idx].Smooth2 == smooth2 && cacheDerivativeOscillator__D3[idx].Smooth3 == smooth3 && cacheDerivativeOscillator__D3[idx].EqualsInput(input))
						return cacheDerivativeOscillator__D3[idx];
			return CacheIndicator<DerivativeOscillator__D3>(new DerivativeOscillator__D3(){ Period = period, Smooth1 = smooth1, Smooth2 = smooth2, Smooth3 = smooth3 }, input, ref cacheDerivativeOscillator__D3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DerivativeOscillator__D3 DerivativeOscillator__D3(int period, int smooth1, int smooth2, int smooth3)
		{
			return indicator.DerivativeOscillator__D3(Input, period, smooth1, smooth2, smooth3);
		}

		public Indicators.DerivativeOscillator__D3 DerivativeOscillator__D3(ISeries<double> input , int period, int smooth1, int smooth2, int smooth3)
		{
			return indicator.DerivativeOscillator__D3(input, period, smooth1, smooth2, smooth3);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DerivativeOscillator__D3 DerivativeOscillator__D3(int period, int smooth1, int smooth2, int smooth3)
		{
			return indicator.DerivativeOscillator__D3(Input, period, smooth1, smooth2, smooth3);
		}

		public Indicators.DerivativeOscillator__D3 DerivativeOscillator__D3(ISeries<double> input , int period, int smooth1, int smooth2, int smooth3)
		{
			return indicator.DerivativeOscillator__D3(input, period, smooth1, smooth2, smooth3);
		}
	}
}

#endregion

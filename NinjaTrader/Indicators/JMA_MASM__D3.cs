// NinjaTrader port of 1998 version of Jurik Moving Average
#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Jurik Moving Average
    /// </summary>
    public class JMA_MASM__D3 : Indicator
    {
		
        #region User Properties
        [NinjaScriptProperty]
        [Display(Description = "Lookback interval", GroupName = "Parameters", Order = 1)]
        public int Length
        {
            get { return length; }
            set { length = Math.Max(1, value); }
        }
		private int length = 7;
          
        [NinjaScriptProperty]
        [Display(Description = "Phase (similar to offset)", GroupName = "Parameters", Order = 1)]
        public double Phase
        {
            get { return phase; }
            set { phase = Math.Max(-100, value); }
        }  
		private double phase = 0;
		
// 		[Browsable(false)]
//    	public string UpColorSerialize
//    	{
//        		get { return Serialize.BrushToString(this.UpColor); }
//        		set { this.UpColor = Serialize.StringToBrush(value); }
//    	}
		
		//[NinjaTrader.Gui.Design.DisplayName("MA rising color")]
		
//		[NinjaScriptProperty]		
//		[Display(Description = "Default Color for Rising Moving Average", GroupName = "Plots", Order = 1)]
//		public Brush UpColor
//        {
//			get { return this.UpBrush; }
//        	set { this.UpBrush = (SolidColorBrush)value; }
//        }
		
//		[Browsable(false)]
//    	public string DownColorSerialize
//    	{
//        		get { return Serialize.BrushToString(this.DownColor); }
//        		set { this.DownColor = Serialize.StringToBrush(value); }
//    	}
		
		//[NinjaTrader.Gui.Design.DisplayName("MA falling color")]
		
//		[NinjaScriptProperty]		
//		[Display(Description = "Default Color for Falling Moving Average", GroupName = "Plots", Order = 1)]
//		public Brush DownColor
//        {
//			get { return this.DownBrush; }
//        	set { this.DownBrush = (SolidColorBrush)value; }
//        }
		
//		[Browsable(false)]
//    	public string NeutralColorSerialize
//    	{
//        		get { return Serialize.BrushToString(this.NeutralColor); }
//        		set { this.NeutralColor = Serialize.StringToBrush(value); }
//    	}
		
		//[NinjaTrader.Gui.Design.DisplayName("Neutral color")]
		
//		[NinjaScriptProperty]		
//		[Display(Description = "Default Color for Neutral", GroupName = "Plots", Order = 1)]
//		public Brush NeutralColor
//        {
//			get { return this.NeutralBrush; }
//        	set { this.NeutralBrush = (SolidColorBrush)value; }
//        }

       #endregion
		
		
		#region program Variables & Properties
		 
		Series<double> JMAValueBuffer, fC0Buffer, fA8Buffer, fC8Buffer;
		//---- temporary buffers
		double[] list, ring1, ring2, buffer;
		int    limitValue, startValue, loopParam, loopCriteria;
		int    cycleLimit, highLimit, counterA, counterB;
		bool initFlag;
		double cycleDelta, lowDValue, highDValue, absValue, paramA, paramB;
		double phaseParam, logParam, JMAValue, series, sValue, sqrtParam, lengthDivider, dValue=0.0;
//---- temporary int variables
		int   s58, s60, s40, s38, s68;
		
		private SolidColorBrush          UpBrush 	  = Brushes.LimeGreen;
		private SolidColorBrush          DownBrush 	  = Brushes.Red;
		private SolidColorBrush          NeutralBrush 	  = Brushes.Yellow;
		
        #endregion
 
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Jurik Moving Average (1998 version)";
				Name										= "JMAMASM";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				//
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "JMA_MASM__D3");
				//	Add(new Plot (new Pen(Color.FromKnownColor(KnownColor.Blue),2), PlotStyle.Dot, "Up"));
				//	Add(new Plot (new Pen(Color.FromKnownColor(KnownColor.Red),2), PlotStyle.Dot, "Down"));
			}
			else if (State == State.Configure)
			{
				
				int i;
				list = new double[128]; // initializes to zero
				ring1 = new double[128];
				ring2 = new double[11];
				buffer = new double[62];
				
				JMAValueBuffer = new Series<double>(this);
				fC0Buffer = new Series<double>(this);
				fA8Buffer = new Series<double>(this);
				fC8Buffer = new Series<double>(this);
				//Trend		= new Series<double>(this);
				
				limitValue = 63; 
				startValue = 64;
				loopParam = loopCriteria = 0;

				for (i = 0; i <= limitValue; i++) list [i] = -1000000; 
				for (i = startValue; i <= 127; i++) list [i] = 1000000; 

				initFlag  = true;
				double lengthParam = (Length < 1.0000000002) ? 0.0000000001 : (Length - 1) / 2.0;

				if (Phase < -100) phaseParam = 0.5;
				else if (Phase > 100) phaseParam = 2.5;
				else phaseParam = Phase / 100.0 + 1.5;

				logParam = Math.Log(Math.Sqrt(lengthParam)) / Math.Log(2.0);
				if (logParam + 2.0 < 0) logParam = 0;
				else logParam += 2.0; 

				sqrtParam = Math.Sqrt(lengthParam) * logParam; 
				lengthParam *= 0.9; 
				lengthDivider = lengthParam / (lengthParam + 2.0);
			}
		}
        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			int i, j, limit = CurrentBar;
			double JMATempValue = (CurrentBar > 0) ? JMAValueBuffer[1] : Input[0];

			series = Input[0];
			if (loopParam < 61) { 
				loopParam++; 
				buffer[loopParam] = series;
			}
			if (loopParam > 30) {
				if (initFlag) {
					initFlag = false;
					int diffFlag = 0;
					for (i = 1; i <= 29; i++) if (buffer[i+1] != buffer[i]) diffFlag = 1;
					highLimit = diffFlag * 30;
					paramB = (highLimit == 0) ? series : buffer[1];
					paramA = paramB;
					if (highLimit > 29) highLimit = 29;
				} else highLimit = 0;
//---- big cycle
			for (i = highLimit; i >= 0; i--) { 
				sValue = (i==0) ? series : buffer[31-i];
				absValue = (Math.Abs(sValue - paramA) > Math.Abs(sValue - paramB)) ?
					Math.Abs(sValue - paramA) : Math.Abs(sValue - paramB); 
				dValue = absValue + 0.0000000001; //1.0e-10;
				if (counterA <= 1) counterA = 127; else counterA--; //starts at 127
				if (counterB <= 1) counterB = 10;  else counterB--; //starts at 10
				if (cycleLimit < 128) cycleLimit++; 
				cycleDelta += (dValue - ring2 [counterB]); 
				ring2 [counterB] = dValue; 
				highDValue = (cycleLimit > 10) ? cycleDelta / 10.0 : cycleDelta / cycleLimit; 

				if (cycleLimit > 127) { 
					dValue = ring1 [counterA]; 
					ring1 [counterA] = highDValue; 
					s68 = 64; s58 = s68; 
					while (s68 > 1) { 
						if (list [s58] < dValue)
							{ s68 /= 2; s58 += s68; }
						else if (list [s58] <= dValue)
						   s68 = 1; 
						else { s68 /= 2; s58 -= s68; }
					}
				} else {
					ring1[counterA] = highDValue; 
					if ((limitValue + startValue) > 127) {
						startValue--; 
						s58 = startValue; 
					} else {
						limitValue++; 
						s58 = limitValue; 
					}
					s38 = (limitValue > 96) ? 96 : limitValue; 
					s40 = (startValue < 32) ? 32 : startValue; 
		      }
//----
				s68 = 64; s60 = s68; 
				while (s68 > 1) {
					if (list[s60] >= highDValue) {
						if (list[s60 - 1] <= highDValue) s68 = 1; 
						else {
							s68 /= 2; 
							s60 -= s68; 
						}
					} else {
						s68 /= 2; 
						s60 += s68; 
					}
					if ((s60 == 127) && (highDValue > list[127])) s60 = 128; 
				}
				if (cycleLimit > 127) {
					if (s58 >= s60) {
						if (((s38 + 1) > s60) && ((s40 - 1) < s60)) 
							lowDValue += highDValue; 
						else if ((s40 > s60) && ((s40 - 1) < s58)) 
							lowDValue += list [s40 - 1]; 
					}
					else if (s40 >= s60) {
						if (((s38 + 1) < s60) && ((s38 + 1) > s58)) 
							lowDValue += list[s38 + 1]; 
					}
					else if ((s38 + 2) > s60) 
						lowDValue += highDValue; 
					else if (((s38 + 1) < s60) && ((s38 + 1) > s58)) 
						lowDValue += list[s38 + 1]; 

					if (s58 > s60) {
						if (((s40 - 1) < s58) && ((s38 + 1) > s58)) 
							lowDValue -= list [s58]; 
						else if ((s38 < s58) && ((s38 + 1) > s60)) 
							lowDValue -= list [s38]; 
					} else {
						if (((s38 + 1) > s58) && ((s40 - 1) < s58)) 
							lowDValue -= list [s58]; 
						else if ((s40 > s58) && (s40 < s60)) 
							lowDValue -= list [s40]; 
					}
				}
				if (s58 <= s60) {
					if (s58 >= s60) list[s60] = highDValue; else {
						for (j = s58 + 1; j <= (s60 - 1); j++) list [j-1] = list[j]; 
						list [s60 - 1] = highDValue; 
					}
				} else {
					for (j = s58 - 1; j >= s60; j--) list [j + 1] = list [j]; 
					list [s60] = highDValue; 
				}

				if (cycleLimit <= 127) {
					lowDValue = 0; 
					for (j = s40; j <= s38; j++) lowDValue += list[j]; 
				}
//----
				if (CurrentBar == 0) {
					fC0Buffer[0] = series;
					JMATempValue = series;
					int leftInt = (Math.Ceiling(sqrtParam) >= 1) ? (int)Math.Ceiling(sqrtParam) : 1; 
					int rightPart = (Math.Floor(sqrtParam) >= 1) ? (int)Math.Floor(sqrtParam) : 1; 
					dValue = (leftInt == rightPart) ? 1.0
						: (sqrtParam - rightPart) / (leftInt - rightPart);
				}

				if ((loopCriteria + 1) > 31) loopCriteria = 31; else loopCriteria++; 
				double sqrtDivider = sqrtParam / (sqrtParam + 1.0);

				if (loopCriteria <= 30) {
					paramA = (sValue - paramA > 0) ? sValue : sValue - (sValue - paramA) * sqrtDivider; 
					paramB = (sValue - paramB < 0) ? sValue : sValue - (sValue - paramB) * sqrtDivider; 
					JMATempValue = series;

					if (loopCriteria == 30) { 
						fC0Buffer[0] = series;
						int intPart = (Math.Ceiling(sqrtParam) >= 1.0) ? (int)Math.Ceiling(sqrtParam) : 1; 
						int dnShift, leftInt = intPart; //IntPortion(intPart); 
						intPart = (Math.Floor(sqrtParam) >= 1.0) ? (int)Math.Floor(sqrtParam) : 1; 
						int upShift, rightPart = intPart; //IntPortion (intPart);
						dValue = (leftInt == rightPart) ? 1.0
							: (sqrtParam - rightPart) / (leftInt - rightPart);
						upShift = (rightPart <= 29) ? rightPart : 29; 
						dnShift = (leftInt <= 29) ? leftInt : 29; 
						fA8Buffer[0] = (series - buffer [loopParam - upShift]) * (1 - dValue) / rightPart + (series - buffer[loopParam - dnShift]) * dValue / leftInt;
					}
				} else {
					double powerValue, squareValue;
					dValue = lowDValue / (s38 - s40 + 1);
					powerValue = (0.5 <= logParam - 2.0) ? logParam - 2.0 : 0.5;
					if (logParam >= Math.Pow(absValue/dValue, powerValue))
						dValue = Math.Pow(absValue/dValue, powerValue);
					else dValue = logParam; 
					if (dValue < 1) dValue = 1;
					powerValue = Math.Pow(sqrtDivider, Math.Sqrt(dValue)); 
					paramA = (sValue - paramA > 0) ? sValue : sValue - (sValue - paramA) * powerValue;
					paramB = (sValue - paramB < 0) ? sValue : sValue - (sValue - paramB) * powerValue; 
   				}
			}
// ---- end of big cycle                  			   
			if (loopCriteria > 30) {
				double powerValue   = Math.Pow(lengthDivider, dValue);
				double squareValue  = powerValue * powerValue;
				fC0Buffer[0] = (1 - powerValue) * series + powerValue * fC0Buffer[1];
				fC8Buffer[0] = (series - fC0Buffer[0]) * (1.0 - lengthDivider) + lengthDivider * fC8Buffer[1];
				fA8Buffer[0] = (phaseParam * fC8Buffer[0] + fC0Buffer[0] - JMATempValue) *
					(powerValue * (-2.0) + squareValue + 1) + squareValue * fA8Buffer[1];  
				JMATempValue += fA8Buffer[0]; 
			}
			JMAValue = JMATempValue;
		} //endif loopparam>30
		if (loopParam <= 30) JMAValue = Input[0];
		else Value[0] = JMAValue;
		JMAValueBuffer[0] = JMAValue;
		
		if (CurrentBar < 2) return;
		
	//	if (Rising(Value))
	//		Up.Set(Value[0]);
	//	else if (Falling(Value))
	//		Down.Set(Value[0]);
		
	//	if (Value[0] > Value[1] && Value[1] < Value[2] && Plots[1].PlotStyle != PlotStyle.Dot)
	//		Up.Set(1, Value[1]);
	//	if (Value[0] < Value[1] && Value[1] > Value[2] && Plots[1].PlotStyle != PlotStyle.Dot)
	//		Down.Set(1, Value[1]);
        }

//		protected override void OnRender
//		(ChartControl chartControl, ChartScale chartScale)
//		{
//			Brush _aa;
//			Exception _ag;
//			try
//			{
//				_aa = Plots[0].Brush;
//				Plots[0].Brush = Brushes.Empty;
//				base.Plot (graphics, bounds, min, max);
//				Plots[0].Pen.Color = _aa;
//				//plot2colorline (graphics, bounds, min, max, Values[0], Plots[0].Pen);
//			}
//			catch (Exception exceptionab)
//			{
//				_ag = exceptionab;
//			}
//		}
		
//		private void plot2colorline (Graphics graphics, Rectangle bounds, double min, double max, Series<double> __4, Stroke __5)
//		{
//			int _aa;
//			double _ab;
//			int _ac;
//			int _ad;
//			Brush _ae;
//			if (Bars == null)
//			{
//				return;
//			}
//			int _af = ChartControl.ChartStyle.GetBarPaintWidth (ChartControl.BarWidth);
//			int _ag = ((Calculate == Calculate.OnBarClose) ? (ChartControl.BarsPainted - 2) : (ChartControl.BarsPainted - 1));
//			int _ah = -1;
//			int _ai = -1;
//			Brush _aj = Brushes.NavajoWhite;
//			Stroke _ak = ((Stroke) __5.Clone ());
//			GraphicsPath _al = ((GraphicsPath) null);
//			for (int _am = 0; true; _am++)
//			{
//				if (_am > _ag)
//				{
//					if (_aj != Brushes.Empty)
//					{
//						_ak.Color = _aj;
//						graphics.DrawPath (_ak, _al);
//					}
//					return;
//				}
//				_aa = (((ChartControl.LastBarPainted - ChartControl.BarsPainted) + 1) + _am);
//				if ((/*NT8 Removed: ChartControl.ShowBarsRequired*/false || ((_aa - Displacement) >= BarsRequiredToPlot)) && (_aa > 0))
//				{
//					_ab = __4[_aa];
					
//					if ((! double.IsNaN (_ab)) && __4.IsValidDataPoint (_aa))
//					{
//						_ac = ((((ChartControl.CanvasRight - ChartControl.BarMarginRight) - (_af / 2)) - ((ChartControl.BarsPainted - 1) * ChartControl.BarSpace)) + (_am * ChartControl.BarSpace));
//						_ad = ((int) (((double) (bounds.Y + bounds.Height)) - (((_ab - min) / NinjaTrader.Gui.Chart.ChartControl.MaxMinusMin (max, min)) * ((double) bounds.Height))));
//						if (_ah >= 0)
//						{
//							_ae = Brushes.Empty;

//							if (__4[((int) (_aa - 1))]> _ab ) _ae = DownColor;
//							else if (__4[((int) (_aa - 1))]< _ab ) _ae = UpColor;
//							else _ae = NeutralColor;

//							if (_ae != _aj)
//							{
//								if ((_al != null) && (_aj != Brushes.Empty))
//								{
//									_ak.Color = _aj;
//									graphics.DrawPath (_ak, _al);
//								}
//								_al = new GraphicsPath ();
//								_aj = _ae;
//							}
//							_al.AddLine (_ah, _ai, _ac, _ad);
//						}
//						_ah = _ac;
//						_ai = _ad;
//					}
//				}
//			}
//		}

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private JMA_MASM__D3[] cacheJMA_MASM__D3;
		public JMA_MASM__D3 JMA_MASM__D3(int length, double phase)
		{
			return JMA_MASM__D3(Input, length, phase);
		}

		public JMA_MASM__D3 JMA_MASM__D3(ISeries<double> input, int length, double phase)
		{
			if (cacheJMA_MASM__D3 != null)
				for (int idx = 0; idx < cacheJMA_MASM__D3.Length; idx++)
					if (cacheJMA_MASM__D3[idx] != null && cacheJMA_MASM__D3[idx].Length == length && cacheJMA_MASM__D3[idx].Phase == phase && cacheJMA_MASM__D3[idx].EqualsInput(input))
						return cacheJMA_MASM__D3[idx];
			return CacheIndicator<JMA_MASM__D3>(new JMA_MASM__D3(){ Length = length, Phase = phase }, input, ref cacheJMA_MASM__D3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.JMA_MASM__D3 JMA_MASM__D3(int length, double phase)
		{
			return indicator.JMA_MASM__D3(Input, length, phase);
		}

		public Indicators.JMA_MASM__D3 JMA_MASM__D3(ISeries<double> input , int length, double phase)
		{
			return indicator.JMA_MASM__D3(input, length, phase);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.JMA_MASM__D3 JMA_MASM__D3(int length, double phase)
		{
			return indicator.JMA_MASM__D3(Input, length, phase);
		}

		public Indicators.JMA_MASM__D3 JMA_MASM__D3(ISeries<double> input , int length, double phase)
		{
			return indicator.JMA_MASM__D3(input, length, phase);
		}
	}
}

#endregion

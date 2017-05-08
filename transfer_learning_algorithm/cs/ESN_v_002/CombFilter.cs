/*
* EchoStateNetwork - Copyright 2016 Benjamin Paaßen
* 
* Theoretical Computer Science for Cognitive Systems
* Cognitive Interaction Technology (CITEC)
* Bielefeld University
*
* A CombFilter for Preprocessing
*/

using System;

namespace EchoState {

	public class CombFilter {

		// the sampling frequency
		public readonly double samplingFreq;
		// the desired frequency to be filtered out
		public readonly double filterFreq;
		// the delay for the comb filter
		private readonly int delay;
		// the damping factor
		private double alpha { get; set; }

		public CombFilter(double samplingFreq, double filterFreq, double alpha) {
			this.alpha        = alpha;
			this.samplingFreq = samplingFreq;
			if(samplingFreq <= 2*filterFreq) throw new ArgumentException("Due to Shannon-Nyström-Theorem, the filter frequency may be at most half of the sampling frequency");
			this.filterFreq   = filterFreq;
			this.delay        = (int) (Math.Round(samplingFreq / filterFreq) - 1);
		}

		public CombFilter(double samplingFreq, double filterFreq) : this(samplingFreq, filterFreq, -0.75) {}

		// Applies the filter to each dimension of the given data.
		public double[,] filter(double[,] data) {
			for(int i = delay; i < data.GetLength(0); i++) {
				for(int c = 0; c < data.GetLength(1); c++) {
					data[i,c] += alpha * data[i-delay,c];
				}
			}
			return data;
		}
	}
}

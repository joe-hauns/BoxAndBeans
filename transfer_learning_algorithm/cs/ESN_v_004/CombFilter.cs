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
//using System.IO;
//using SimpleJSON;

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

		// buffer arrays to be able to continue processing data 
		private double[,] raw_memory;
		private double[,] filtered_memory;


		public CombFilter(double samplingFreq, double filterFreq, double alpha, int numChannels) {
			this.alpha        = alpha;
			this.samplingFreq = samplingFreq;
			if(samplingFreq <= 2*filterFreq) throw new ArgumentException("Due to Shannon-Nyström-Theorem, the filter frequency may be at most half of the sampling frequency");
			this.filterFreq   = filterFreq;
			this.delay        = (int) (Math.Round(samplingFreq / filterFreq));
			this.raw_memory   = new double[this.delay, numChannels];
			this.filtered_memory = new double[this.delay, numChannels];
		}

		public CombFilter(double samplingFreq, double filterFreq) : this(samplingFreq, filterFreq, 0.75, 8) {}

		// Applies the filter to each dimension of the given data.
		public double[,] filter(double[,] data) {
			double[,] filtered = new double[data.GetLength(0), data.GetLength(1)];
			// filter data before the delay using the memory
			for(int t = 0; t < delay; t++) {
				for(int c = 0; c < data.GetLength(1); c++) {
					filtered[t, c] = data[t, c] - raw_memory[t, c] + alpha * filtered_memory[t, c];
				}
			}
			// process rest of the data
			for(int t = delay; t < data.GetLength(0); t++) {
				for(int c = 0; c < data.GetLength(1); c++) {
					filtered[t, c] = data[t, c] - data[t - delay, c] + alpha * filtered[t - delay, c];
				}
			}
			// put new data into memory
			for(int t = 0; t < delay; t++) {
				for(int c = 0; c < raw_memory.GetLength(1); c++) {
					raw_memory[t, c]      = data[data.GetLength(0) - delay + t, c];
					filtered_memory[t, c] = filtered[data.GetLength(0) - delay + t, c];
				}
			}
			return filtered;
		}

		public void reset() {
			for(int t = 0; t < delay; t++) {
				for(int c = 0; c < raw_memory.GetLength(1); c++) {
					raw_memory[t, c]      = 0;
					filtered_memory[t, c] = 0;
				}
			}
		}

		/*
		// A test function to ensure that the feature computation is the same as in Matlab
		public static void Main() {
			// Load example raw data from a JSON file
			string raw_data_string          = FileRead.readFileData("raw.json");
			double[,] raw                   = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) JSONNode.Parse(raw_data_string))["raw"]
			);
			// Load expected filtered data for raw data from JSON file
			raw_data_string                 = FileRead.readFileData("filtered.json");
			double[,] expected_filtered     = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) JSONNode.Parse(raw_data_string))["filtered"]
			);
			// process raw data
			CombFilter filter               = new CombFilter(200, 50);
			double[,] actual_filtered       = filter.filter(raw);

			int t = 0;
			while(t < 20) {
				double[] actual   = DenseMatrix.Row(actual_filtered, t);
				double[] expected = DenseMatrix.Row(expected_filtered, t);
				bool err            = false;
				for(int c = 0; c < raw.GetLength(1); c++) {
					if(Math.Abs(actual[c] - expected[c]) > 0.001) {
						err = true;
						break;
					}
				}
				if(err) {
					System.Console.WriteLine("Error at time " + t + ". Expected:");
					DenseMatrix.DisplayVector(expected);
					System.Console.WriteLine("but was:");
					DenseMatrix.DisplayVector(actual);
				}
				t++;
			}
		}*/
	}
}

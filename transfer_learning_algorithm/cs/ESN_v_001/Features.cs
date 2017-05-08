/*
* EchoStateNetwork - Copyright 2016 Benjamin Paaßen
* 
* Theoretical Computer Science for Cognitive Systems
* Cognitive Interaction Technology (CITEC)
* Bielefeld University
*
* Feature computation for machine learning methods
*/

using System;
using System.Collections.Generic;

namespace EchoState {
	public class Features {
		private Features() {}

		// Computes a standard feature matrix for the
		// given raw data,
		// Let N be the number of frames in the raw
		// data, W be the number of frames for a window,
		// O be the number of overlap frames and C
		// be the number of channels. Then the result
		// is a list of (C * F) x 1 vectors with  (N / W + 1) entries
		// list, where F is the number of features.
		//
		// The standard features include:
		// * tvar : the variance for each channel across
		// the time frame.
		public static List<double[]> extractFeatures(double[,] raw_data, int W, int O) {
			int F = 1;
			int N = raw_data.GetLength(0);
			int C = raw_data.GetLength(1);
			List<double[]> X = new List<double[]>();
			{
			int i = 0;
			while(i < N) {
				int lo     = Math.Max(0, i - O);
				int hi     = Math.Min(N, i + W + O);
				double[] x = new double[F * C];
				tvar(raw_data, lo, hi, F, 0, x);
				X.Add(x);
				i += W;
			}
			}
			return X;
		}

		// Expects as input a W x C matrix of raw data,
		// where W is the number of frames in one time
		// window and C is the number of EMG channels
		// and computes the variance for each
		// channel in this time window.
		// The result is stored in the given x vector
		// at the positions f, f+F, f+2*F , ..., f+(C-1)*F
		// where w is the index of the current window,
		// f is the index of this feature and
		// F is the overall number of features.
		// Accordingly, features is expected to be a
		// F * C x 1 vector.
		public static void tvar(double[,] raw, int lo, int hi, int F, int f, double[] x) {
			int C = raw.GetLength(1);
			int N = hi - lo - 1;
			// tmp variable for deviation
			double d;
			for(int c = 0; c < C; c++) {
				// compute mean first
				double mean = 0;
				for(int i = lo; i < hi; i++) {
					mean += raw[i, c];
				}
				mean /= N;
				// then compute variance
				for(int i = lo; i < hi; i++) {
					d = raw[i, c] - mean;
					x[f + c*F] += d * d;
				}
				x[f + c*F] /= N;
			}
		}
	}
}

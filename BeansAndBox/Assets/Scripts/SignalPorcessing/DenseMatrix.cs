/*
* EchoStateNetwork - Copyright 2016 Benjamin Paaßen
*
* Theoretical Computer Science for Cognitive Systems
* Cognitive Interaction Technology (CITEC)
* Bielefeld University
*
* Some Utility Functions for dense matrices
*/
using System;
using SimpleJSON;

namespace EchoState {

	public class DenseMatrix {
		private DenseMatrix() {}

		// multiplies the given dense matrix (from the left side) with the given vector.
		public static double[] multiply(double[,] W, double[] x) {
			if(W.GetLength(1) != x.Length) throw new ArgumentException("The given matrix has "
				+ W.GetLength(1) + " columns but the vector has " + x.Length + " entries!");
			double[] y = new double[W.GetLength(0)];
			for(int i = 0; i < W.GetLength(0); i++) {
				for(int j = 0; j < x.Length; j++) {
					y[i] += W[i,j] * x[j];
				}
			}
			return y;
		}

		// multiplies the given first dense matrix (from the left side) with the given right dense
		// matrix
		public static double[,] multiply(double[,] T, double[,] W) {
			if(T.GetLength(1) != W.GetLength(0)) throw new ArgumentException("The given first matrix has "
				+ T.GetLength(1) + " columns but the given second input matrix has " + W.GetLength(0) + " rows!");
			double[,] W2 = new double[T.GetLength(0), W.GetLength(1)];
			for(int i = 0; i < T.GetLength(0); i++) {
				for(int k = 0; k < T.GetLength(1); k++) {
					for(int j = 0; j < W.GetLength(1); j++) {
						W2[i, j] += T[i, k] * W[k, j];
					}
				}
			}
			return W2;
		}

		// reads a dense matrix (2-dimensional double array) from a JSON array
		public static double[,] FromJSON(JSONArray jW) {
			int M = jW.Count;
			int N = jW[0].Count;
			double[,] W = new double[M,N];
			for(int i = 0; i < M; i++) {
				if(jW[i].Count != N) throw new ArgumentException(
					"Inconsistent matrix size: Expected " + N + " columns but found "
					+ jW[i].Count + " columns in row " + i);
				for(int j = 0; j < N; j++) {
					W[i,j] = jW[i][j].AsDouble;
				}
			}
			return W;
		}

		// reads a double vector from a JSON array
		public static double[] VectorFromJSON(JSONArray jv) {
			int M      = jv.Count;
			double[] v = new double[M];
			for(int i = 0; i < M; i++) {
				v[i] = jv[i].AsDouble;
			}
			return v;
		}

		// reads an int vector from a JSON array
		public static int[] IntVectorFromJSON(JSONArray jv) {
			int M      = jv.Count;
			int[] v    = new int[M];
			for(int i = 0; i < M; i++) {
				v[i] = jv[i].AsInt;
			}
			return v;
		}

/*
		// Displays a dense matrix
		public static void Display(double[,] W) {
			System.Console.WriteLine();
			// print column headers
			System.Console.Write("i\\j\t|");
			for (int j = 0; j < W.GetLength(1); j++) {
				System.Console.Write(j);
				System.Console.Write("\t|");
			}
			System.Console.WriteLine();
			// print border
			for (int j = 0; j < W.GetLength(1)+1; j++) {
				System.Console.Write("--------");
			}
			System.Console.WriteLine("-");
			// print data
			for (int i = 0; i < W.GetLength(0) ; i++) {
				// print row header
				System.Console.Write(i);
				// print entries
				for (int j = 0; j < W.GetLength(1); j++) {
					System.Console.Write("\t|" + Math.Round(W[i,j], 3));
				}
				System.Console.WriteLine();
			}
			System.Console.WriteLine();
		}

		public static void DisplayVector(double[] v) {
			if(v.Length == 0) return;
			System.Console.Write("[");
			System.Console.Write(v[0]);
			for(int i = 1; i < v.Length; i++) {
				System.Console.Write(", ");
				System.Console.Write(v[i]);
			}
			System.Console.WriteLine("]");
		}*/

		// Selects a single row from a dense matrix
		// Be advised that this requires data copy and is _not_ a reference call!
		public static double[] Row(double[,] W, int i) {
			int N = W.GetLength(1);
			double[] row = new double[N];
			for(int j = 0; j < N; j++) {
				row[j] = W[i, j];
			}
			return row;
		}
	}
}

/*
* EchoStateNetwork - Copyright 2016 Benjamin Paaßen
* 
* Theoretical Computer Science for Cognitive Systems
* Cognitive Interaction Technology (CITEC)
* Bielefeld University
*
* A simple SparseMatrix implementation (as compressed row storage)
* for the purpose of storing the weights of an Echo State Network.
* Also supports loading and storing the matrix in a CSV file.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using SimpleJSON;

namespace EchoState {

	public class SparseEntry {
		readonly public int i;
		readonly public int j;
		readonly public double val;

		public SparseEntry(int i, int j, double val) {
			this.i   = i;
			this.j   = j;
			this.val = val;
		}
	}

	public class WeightMatrix {
		// The actual values contained in this matrix.
		readonly private double[] values;
		// the column index of entry i in the values array.
		readonly private int[] col_idxs;
		// an array to the start index for values in row i.
		readonly private int[] row_ptr;

		readonly private int M;
		readonly private int N;

		// Constructor for testing purposes. Initializes a sparse matrix 
		public WeightMatrix(List<SparseEntry> entries, int M, int N) {
			this.M = M;
			this.N = N;

			// sort the entries first; tuple sorts use lexicographic ordering
			entries.Sort((x, y) => compareTo(x, y));

			// initialize the internal arrays
			this.values     = new double[entries.Count];
			this.col_idxs   = new int[entries.Count];

			this.row_ptr    = new int[M+1];
			// fill the entries
			int i = 0;
			int k = 0;
			foreach(SparseEntry entry in entries) {
				if(entry.i >= this.M) throw new IndexOutOfRangeException(entry.i + " is out of range of the matrix with " + M + " rows!");
				// update the row ptr if necessary
				if(entry.i > i) {
					i++;
					for(;i<=entry.i;i++) {
						this.row_ptr[i] = k;
					}
					i--;
				}
				// set the column index
				if(entry.j >= this.N) throw new IndexOutOfRangeException(entry.j + " is out of range of the matrix with " + N + " columns!");
				this.col_idxs[k] = entry.j;
				// set the value
				this.values[k]   = entry.val;
				k++;
			}
			i++;
			for(;i < M+1; i++) {
				this.row_ptr[i] = entries.Count;
			}
		}

		private static int compareTo(SparseEntry x, SparseEntry y) {
			if(x.i < y.i) return -1;
			if(x.i > y.i) return 1;
			if(x.j < y.j) return -1;
			if(x.j > y.j) return 1;
			return 0;
		}

		// Creates a WeightMatrix object from JSON data. The expected format is
		//  {
		//      "_ArraySize_": [3,3],
		//      "_ArrayData_": [
		//          [i_1, j_1, v_1],
		//          [i_2, j_2, v_2],
		//          ...,
		//          [i_K, j_K, v_K]
		//      ]
		//  }
		// where _ArraySize_ contains the number of rows and the number of columns, 
		// K is the number of nonzero entries, i_k is the row index of the
		// kth nonzero entry, i_k is the column index of the kth nonzero entry
		// and v_k is the value of the kth nonzero entry.
		//
		// Note: We expect the input indices to start at 1, not at zero!
		public WeightMatrix(JSONClass jW) : this(
			fromJSON((JSONArray) jW["_ArrayData_"]),
			((JSONArray) jW["_ArraySize_"])[0].AsInt,
			((JSONArray) jW["_ArraySize_"])[1].AsInt
		) {}

		private static List<SparseEntry> fromJSON(JSONArray jEntries) {
			List<SparseEntry> entries = new List<SparseEntry>();
			for(int k = 0; k < jEntries.Count; k++) {
				entries.Add(new SparseEntry(jEntries[k][0].AsInt - 1, jEntries[k][1].AsInt - 1, jEntries[k][2].AsDouble));
			}
			return entries;
		}

		// returns the number of rows in this WeightMatrix.
		public int NumRows() {
			return M;
		}

		// returns the number of columns in this WeightMatrix.
		public int NumColumns() {
			return N;
		}

		// Applies this weight matrix to a state, that is: This matrix W
		// is multiplied from the left to the state vector.
		public double[] Apply(double[] state) {
			if(state.Length != this.N) {
				throw (new ArgumentException("The given state variable has " + state.Length + " neurons but this Weight matrix is created for " + N + " neurons."));
			}
			double[] newState = new double[N];
			for(int i = 0; i < M; i++) {
				for(int k = this.row_ptr[i]; k < this.row_ptr[i+1]; k++) {
					newState[i] += this.values[k] * state[this.col_idxs[k]];
				}
			}
			return newState;
		}
/*
		public void Display() {
			Display(this.values.Length);
		}

		public void Display(int K) {
			int i = -1;
			for(int k = 0; k < K; k++) {
				if(k >= row_ptr[i+1]){
					i++;
					System.Console.WriteLine("Start of row: " + i);
				}
				System.Console.Write(col_idxs[k]);
				System.Console.Write(" : ");
				System.Console.WriteLine(this.values[k]);
			}
		}

		// This should only be used for testing purposes and small matrices!
		// Displays a dense version of the matrix.
		public void DisplayDense() {

			System.Console.WriteLine();
			// print column headers
			System.Console.Write("i\\j");
			for (int j = 0; j < N; j++) {
				System.Console.Write("\t|");
				System.Console.Write(j);
			}
			System.Console.WriteLine();
			// print border
			for (int j = 0; j < N+1; j++) {
				System.Console.Write("--------");
			}
			System.Console.WriteLine("-");
			// print data
			for (int i = 0; i < M ; i++) {
				// print row header
				System.Console.Write(i);
				int j = 0;
				// print entries
				for (int k = this.row_ptr[i]; k < this.row_ptr[i+1]; k++) {
					// print zeros for empty entries
					for (; j < this.col_idxs[k]; j++) {
						System.Console.Write("\t|0");
					}
					// print the actual entry
					System.Console.Write("\t|" + Math.Round(this.values[k], 3));
					j++;
				}
				// finish the row with zero entries
				for (; j < N; j++) {
					System.Console.Write("\t|0");
				}
				System.Console.WriteLine();
			}
			System.Console.WriteLine();
		}*/
	}
}

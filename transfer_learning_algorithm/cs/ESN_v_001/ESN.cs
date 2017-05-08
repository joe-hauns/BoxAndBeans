/*
* EchoStateNetwork - Copyright 2016 Benjamin Paaßen
* 
* Theoretical Computer Science for Cognitive Systems
* Cognitive Interaction Technology (CITEC)
* Bielefeld University
*
* An Echo State Network implementation without training but with classification abilities,
* provided that a weight matrix is given.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;

namespace EchoState {
	public class EchoStateNetwork {

		// the input weights of the network
		private readonly double[,] W_in;
		// the internal weights of the network
		private readonly WeightMatrix W;
		// the output weights of the network
		private double[,] W_out {get; set;}
		// The dt parameter, which steers how much of the old state of the network is kept. For dt = 1, the old
		// state has no influence at all, while for dt=0 the network is constant.
		private double _dt;

		public double dt {
			get {
				return _dt;
			}
			set {
				if(value < 0 || value > 1) {
					throw new ArgumentOutOfRangeException("dt has to be in the range [0,1]");
				}
				_dt = value;
			}
		}

		// a normalization offset for input data
		private double[] _inpOffset;
		public double[] inpOffset {
			get{ return _inpOffset; }
			set{
				if(value.Length != NumInputs()) throw (new ArgumentException(
					"Expected one offset for each off the " + NumInputs() + " inputs, but got " + value.Length + " offsets!"));
				_inpOffset = value;
			}
		}
		// The normalization range for input data
		private double[,] _inpRange;
		public double[,] inpRange {
			get { return _inpRange; }
			set {
				if(value.GetLength(1) != NumInputs()) throw (new ArgumentException(
					"Expected a range for each of the " + NumInputs() + " inputs, but got " + value.GetLength(1) + " ranges!"));
				if(value.GetLength(0) != 2) throw (new ArgumentException(
					"A range consists of a lower limit and an upper limit, but the given array has " + value.GetLength(0) + " columns!"));
				for(int i = 0; i < NumInputs(); i++) {
					if(value[0, i] > value[1, i]) throw (new ArgumentException(
						"For input dimension " + i + " the given lower bound of the range was bigger than the upper bound, which is invalid!"));
				}
				_inpRange = value;
			}
		}

		// The current state of the network.
		public double[] state { get; set; }
		// The current state of the network prior to tanh
		public double[] a { get; set; }

		// Creates an EchoStateNetwork witht he given parameters.
		// Let K be the number of inputs, M be the number of hidden neurons and N be the number of outputs,
		// then:
		// * W_in  is expected to be a dense M x K matrix, which maps the input to the hidden neurons.
		// * W     is expected to be a sparse M x M matrix, which maps the hidden neurons to the hidden neurons.
		// * W_out is expected to be a dense N x M matrix, which maps the hidden neurons to the output.
		// Only the latter matrix can be changed later on and is adjusted to the training data.
		public EchoStateNetwork(double[,] W_in, WeightMatrix W, double[,] W_out) {
			this.W_in = W_in;
			if(W_in.GetLength(0) != W.NumRows()) {
				throw (new ArgumentException("The given input weight matrix projects to "
					+ W_in.GetLength(0) + " neurons but the internal weight matrix expects " + W.NumRows() + " neurons."));
			}
			if(W.NumRows() != W.NumColumns()) {
				throw (new ArgumentException("The given internal weight matrix is not square."));
			}
			if(W.NumColumns() != W_out.GetLength(1)) {
				throw (new ArgumentException("The given output weight matrix projects to " + W.NumColumns() +
					" but the given output weight matrix expects " + W_out.GetLength(1) + " neurons."));
			}
			this.W     = W;
			this.W_out = W_out;
			this.state = new double[W.NumColumns()];
			this.a     = new double[W.NumColumns()];
			this.dt    = 1;
		}

		// Reads an EchoStateNetwork from JSON files, one for the reservoir (meta parameters, input weights and internal weights)
		// and one for the output weights.
		public static EchoStateNetwork FromJSON(string reservoir_file, string W_out_file) {
			string reservoir_data = readFileData(reservoir_file);
			string out_data = readFileData(W_out_file);
			return FromJSON(
				(JSONClass) ((JSONClass) JSONNode.Parse(reservoir_data))["esn"],
				(JSONArray) ((JSONClass) JSONNode.Parse(out_data))["esn"]
			);
		}

		// reads all data in a file and puts it into a string
		private static string readFileData(string path) {
			StringBuilder sb = new StringBuilder();
			FileStream file  = new FileStream(path, FileMode.Open);
			using (StreamReader sr = new StreamReader(file, System.Text.Encoding.Default)) {
				string line;
				while((line = sr.ReadLine()) != null) {
					sb.Append(line);
				}
			}
			return sb.ToString();
		}

		// converts the JSON representation of a reservoir and an output weight matrix
		// to an EchoStateNetwork
		public static EchoStateNetwork FromJSON(JSONClass jReservoir, JSONArray jW_out) {
			double[,] W_in  = DenseMatrixFromJSON((JSONArray) jReservoir["wInp"]);
			WeightMatrix W  = new WeightMatrix((JSONClass) jReservoir["wRes"]);
			double[,] W_out = DenseMatrixFromJSON(jW_out);
			EchoStateNetwork network = new EchoStateNetwork(W_in, W, W_out);
			// read meta parameters
			network.dt       = jReservoir["dt"].AsDouble;
			network.inpOffset = VectorFromJSON((JSONArray) jReservoir["inpOffset"]);
			network.inpRange  = DenseMatrixFromJSON((JSONArray) jReservoir["inpRange"]);
			return network;
		}

		// reads a dense matrix (2-dimensional double array) from a JSON array
		public static double[,] DenseMatrixFromJSON(JSONArray jW) {
			int M = jW.Count;
			int N = jW[0].Count;
			double[,] W = new double[M,N];
			for(int i = 0; i < M; i++) {
				if(jW[i].Count != N) throw new IndexOutOfRangeException(
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

		// returns the number of inputs for this ESN
		public int NumInputs() {
			return this.W_in.GetLength(1);
		}

		// returns the number of hidden neurons of this ESN
		public int NumNeurons() {
			return this.W.NumColumns();
		}

		// returns the number of outputs of this ESN
		public int NumOutputs() {
			return this.W_out.GetLength(0);
		}

		// Updates the internal state of this ESN according to the given input and
		// the current state. If x is the state vector and a is the input vector,
		// the formula for the update is:
		//
		// x' = tanh(dt * (W_in * a + W * x) + (1-dt) * x)
		//
		// where W_in is the input weight matrix and W is the internal weight matrix
		public void Update(double[] input) {
			if(input.Length != NumInputs()) {
				throw (new ArgumentException("The given input weight matrix expects " + W_in.GetLength(0) +
					" inputs but the given input vector has " + input.Length + " entries."));
			}
			// normalize the input
			for(int i = 0; i < NumInputs(); i++) {
				input[i] = 2 * (input[i] - inpOffset[i] - inpRange[0, i]) / (inpRange[1, i] - inpRange[0, i]) - 1;
			}

			// update using the recurrent connections first.
			double[] newState  = W.Apply(this.state);
			// compute the new input
			double[] inputState = matrixMult(this.W_in, input);
			for(int i = 0; i < NumNeurons(); i++) {
				// TODO: One could also consider a bias value here, but with all
				// net configurations we had until now the bias was zero.
				// compute the new internal state before ...
				this.a[i]     = dt * (inputState[i] + newState[i]) + (1-dt) * this.a[i];
				// ... and after the tanh nonlinearity
				this.state[i] = Math.Tanh(this.a[i]);
			}
		}

		// Computes the output of the ESN which follows from the current state
		// The formula for this output is
		// W_out * x
		// where W_out is the output weight matrix and x is the current state vector.
		public double[] GetOutput() {
			return matrixMult(this.W_out, this.state);
		}

		// Returns a discretized input from the set {-1, 0, 1}^N where N is the number of
		// outputs. This is essentially just a rounded version of the GetOutput() return
		// value
		public double[] GetOutputClassification() {
			double[] y = GetOutput();
			for(int i = 0; i < y.Length; i++) {
				if(y[i] < -0.5) {
					y[i] = -1;
				} else if(y[i] < 0.5) {
					y[i] = 0;
				} else {
					y[i] = 1;
				}
			}
			return y;
		}

		// multiplies the given dense matrix with the given vector.
		private static double[] matrixMult(double[,] W, double[] x) {
			double[] y = new double[W.GetLength(0)];
			for(int i = 0; i < W.GetLength(0); i++) {
				for(int j = 0; j < x.Length; j++) {
					y[i] += W[i,j] * x[j];
				}
			}
			return y;
		}

		// This should only be used for testing in small networks: Displays the network weights as dense matrices
		public void Display() {
			System.Console.WriteLine("NumInputs: " + NumInputs() + ", NumNeurons: " + NumNeurons() + ", NumOutputs: " + NumOutputs());
			System.Console.WriteLine("dt: " + dt);
			System.Console.WriteLine("Input offsets: [" + String.Join(", ", inpOffset) + "]");
			System.Console.WriteLine("Input ranges: ");
			for(int i = 0; i < NumInputs(); i++) {
				System.Console.WriteLine("[" + inpRange[0, i] + ", " + inpRange[1, i] + "]");
			}
			System.Console.Write("Input Weights:");
			DisplayDenseMatrix(W_in);
			System.Console.Write("Internal Weights:");
			this.W.DisplayDense();
			System.Console.Write("Output Weights:");
			DisplayDenseMatrix(W_out);
		}

		private static void DisplayDenseMatrix(double[,] W) {
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

		// A very simple Main method for testing purposes
		public static void Main() {
			// Load an example reservoir from JSON files
			EchoStateNetwork network = FromJSON("fullESN.json", "outputESN2.json");
			System.Console.WriteLine("W:");
			network.W.Display(20);

			// load some raw data
			string raw_data_string = readFileData("raw.json");
			double[,] raw_data     = DenseMatrixFromJSON(
				(JSONArray) ((JSONClass) JSONNode.Parse(raw_data_string))["raw"]
			);

			// extract features
			List<double[]> X = Features.extractFeatures(raw_data, 10, 5);
			// Let the network run on this data
			System.Console.WriteLine("y = " + String.Join(", ", network.GetOutput()));

			int t = 0;
			foreach(double[] x in X) {
				System.Console.WriteLine("t = " + t);
				System.Console.WriteLine("x = [" + String.Join(", ", x) + "]");
				network.Update(x);
				System.Console.WriteLine("y = [" + String.Join(", ", network.GetOutput()) + "]");
				t++;
			}
		}
	}
}

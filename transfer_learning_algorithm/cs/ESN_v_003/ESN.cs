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
		public readonly double[,] W_in;
		// the internal weights of the network
		public readonly WeightMatrix W;
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

		// Creates an EchoStateNetwork with the given parameters.
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
			double[,] W_in  = DenseMatrix.FromJSON((JSONArray) jReservoir["wInp"]);
			WeightMatrix W  = new WeightMatrix((JSONClass) jReservoir["wRes"]);
			double[,] W_out = DenseMatrix.FromJSON(jW_out);
			EchoStateNetwork network = new EchoStateNetwork(W_in, W, W_out);
			// read meta parameters
			network.dt       = jReservoir["dt"].AsDouble;
			network.inpOffset = DenseMatrix.VectorFromJSON((JSONArray) jReservoir["inpOffset"]);
			network.inpRange  = DenseMatrix.FromJSON((JSONArray) jReservoir["inpRange"]);
			// check output range
			double[] outOffset = DenseMatrix.VectorFromJSON((JSONArray) jReservoir["outOffset"]);
			for(int i = 0; i < outOffset.Length; i++) {
				if(Math.Abs(outOffset[i]) > 1E-5) throw new NotImplementedException("Nonzero values of outOffset are not supported at the moment! " + outOffset[i]);
			}
			double[,] outRange = DenseMatrix.FromJSON((JSONArray) jReservoir["outRange"]);
			for(int i = 0; i < outRange.GetLength(1); i++) {
				if(Math.Abs(outRange[0, i] + 1) > 1E-3) throw new NotImplementedException("Other output ranges than [-1, 1] are not supported at the moment! Error: " + (Math.Abs(outRange[0, i] + 1)));
				if(Math.Abs(outRange[1, i] - 1) > 1E-3) throw new NotImplementedException("Other output ranges than [-1, 1] are not supported at the moment! Error: " + (Math.Abs(outRange[1, i] - 1)));
			}
			return network;
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
		// x' = dt * tanh(W_in * a + W * x) + (1-dt) * x
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
			double[] inputState = DenseMatrix.multiply(this.W_in, input);
			for(int i = 0; i < NumNeurons(); i++) {
				// TODO: One could also consider a bias value here, but with all
				// net configurations we had until now the bias was zero.
				// compute the new internal state before the nonlinearity
				double a_i    = inputState[i] + newState[i];
				// apply the tanh nonlinearity
				double h_i    = Math.Tanh(a_i);
				// compute the new state as an interpolation between the new h_i value and the old state.
				this.state[i] = dt * h_i + (1-dt) * this.state[i];
			}
		}

		// Computes the output of the ESN which follows from the current state
		// The formula for this output is
		// W_out * x
		// where W_out is the output weight matrix and x is the current state vector.
		public double[] GetOutput() {
			return DenseMatrix.multiply(this.W_out, this.state);
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
/*
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
			DenseMatrix.Display(W_in);
			System.Console.Write("Internal Weights:");
			this.W.DisplayDense();
			System.Console.Write("Output Weights:");
			DenseMatrix.Display(W_out);
		}

		// A very simple Main method for testing purposes
		public static void Main() {
			// Load an example reservoir from JSON files
			EchoStateNetwork network = FromJSON("reservoir.json", "out.json");
			System.Console.WriteLine("W:");
			network.W.Display(20);

			// load some raw data
			string raw_data_string = readFileData("raw.json");
			double[,] raw_data     = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) JSONNode.Parse(raw_data_string))["raw"]
			);

			// apply the network to the data
			for(int t = 0; t < raw_data.GetLength(0); t++) {
				double[] x = DenseMatrix.Row(raw_data, t);
				network.Update(x);
				//DenseMatrix.DisplayVector(network.state);
				System.Console.Write("y_" + (t+1) + "=");
				DenseMatrix.DisplayVector(network.GetOutput());
			}
		}*/
	}
}

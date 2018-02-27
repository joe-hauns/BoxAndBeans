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

		// a normalization offset for output data
		private double[] _outOffset;
		public double[] outOffset {
			get{ return _outOffset; }
			set{
				if(value.Length != NumOutputs()) throw (new ArgumentException(
					"Expected one offset for each off the " + NumOutputs() + " outputs, but got " + value.Length + " offsets!"));
				_outOffset = value;
			}
		}
		// The normalization range for output data
		private double[,] _outRange;
		public double[,] outRange {
			get { return _outRange; }
			set {
				if(value.GetLength(1) != NumOutputs()) throw (new ArgumentException(
					"Expected a range for each of the " + NumOutputs() + " outputs, but got " + value.GetLength(1) + " ranges!"));
				if(value.GetLength(0) != 2) throw (new ArgumentException(
					"A range consists of a lower limit and an upper limit, but the given array has " + value.GetLength(0) + " columns!"));
				for(int i = 0; i < NumOutputs(); i++) {
					if(value[0, i] > value[1, i]) throw (new ArgumentException(
						"For input dimension " + i + " the given lower bound of the range was bigger than the upper bound, which is invalid!"));
				}
				_outRange = value;
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
			string reservoir_data = FileRead.readFileData(reservoir_file);
			string out_data = FileRead.readFileData(W_out_file);
			return FromJSON(
				(JSONClass) ((JSONClass) JSONNode.Parse(reservoir_data))["esn"],
				(JSONClass) ((JSONClass) JSONNode.Parse(out_data))["esn"]
			);
		}

		// converts the JSON representation of a reservoir and an output weight matrix
		// to an EchoStateNetwork
		public static EchoStateNetwork FromJSON(JSONClass jReservoir, JSONClass jOut) {
			// read input weights
			double[,] W_in    = DenseMatrix.FromJSON((JSONArray) jReservoir["wInp"]);
			// read reservoir matrix
			WeightMatrix W    = new WeightMatrix((JSONClass) jReservoir["wRes"]);
			// read output weights
			double[,] W_out   = DenseMatrix.FromJSON((JSONArray) jOut["wOut"]);
			// create network object
			EchoStateNetwork network = new EchoStateNetwork(W_in, W, W_out);
			// read meta parameters
			network.dt        = jReservoir["dt"].AsDouble;
			network.inpOffset = DenseMatrix.VectorFromJSON((JSONArray) jReservoir["inpOffset"]);
			network.inpRange  = DenseMatrix.FromJSON((JSONArray) jReservoir["inpRange"]);
			network.outOffset = DenseMatrix.VectorFromJSON((JSONArray) jOut["outOffset"]);
			network.outRange  = DenseMatrix.FromJSON((JSONArray) jOut["outRange"]);
			return network;
		}

		// Reads an EchoStateNetwork from JSON files, one for the reservoir (meta parameters, input weights and internal weights)
		// one for the transfer matrix, and one for the output weights.
		public static EchoStateNetwork FromJSON(string reservoir_file, string transfer_file, string W_out_file) {
			string reservoir_data = FileRead.readFileData(reservoir_file);
			string transfer_data = FileRead.readFileData(transfer_file);
			string out_data = FileRead.readFileData(W_out_file);
			return FromJSON(
				(JSONClass) ((JSONClass) JSONNode.Parse(reservoir_data))["esn"],
				(JSONClass) JSONNode.Parse(transfer_data),
				(JSONClass) ((JSONClass) JSONNode.Parse(out_data))["esn"]
			);
		}

		// converts the JSON representation of a reservoir and an output weight matrix
		// to an EchoStateNetwork
		public static EchoStateNetwork FromJSON(JSONClass jReservoir, JSONClass jT, JSONClass jOut) {
			// read input weights
			double[,] W_in    = DenseMatrix.FromJSON((JSONArray) jReservoir["wInp"]);
			// read the transfer matrix
			double[,] T       = DenseMatrix.FromJSON((JSONArray) jT["transfMap"]);
			// multiply W_in with T
			W_in              = DenseMatrix.multiply(W_in, T);
			// read reservoir matrix
			WeightMatrix W    = new WeightMatrix((JSONClass) jReservoir["wRes"]);
			// read output weights
			double[,] W_out   = DenseMatrix.FromJSON((JSONArray) jOut["wOut"]);
			// create network object
			EchoStateNetwork network = new EchoStateNetwork(W_in, W, W_out);
			// read meta parameters
			network.dt        = jReservoir["dt"].AsDouble;
			network.inpOffset = DenseMatrix.VectorFromJSON((JSONArray) jReservoir["inpOffset"]);
			network.inpRange  = DenseMatrix.FromJSON((JSONArray) jReservoir["inpRange"]);
			network.outOffset = DenseMatrix.VectorFromJSON((JSONArray) jOut["outOffset"]);
			network.outRange  = DenseMatrix.FromJSON((JSONArray) jOut["outRange"]);
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
			double[] y = DenseMatrix.multiply(this.W_out, this.state);
			// normalize the output
			for(int i = 0; i < y.Length; i++) {
				y[i] = 0.5 * (outRange[1, i] - outRange[0, i]) * (y[i] + 1) + outRange[0, i] + outOffset[i];
			}
			return y;
		}

		// Computes the output of the ESN but clips and normalizes it such that
		// it is guaranteed to be in the range [-1, 1] in each dimension and
		// values below 0.2 are ignored
		public double[] GetNormalizedOutput() {
			double[] y = GetOutput();
			for(int i = 0; i < y.Length; i++) {
				var x = Math.Min(Math.Max(y [i], -1), 1);
				if (x < - 0.2) {
					y[i] = 2 * (y[i] + 0.2);
				} else if (x > 0.2) {
					y[i] = 2 * (y[i] - 0.2);
				} else {
					y [i] = 0;
				}
			}
			return y;
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
			EchoStateNetwork network = FromJSON("current_reservoir.json", "current_output_weights.json");
			//System.Console.WriteLine("W:");
			//network.W.Display(20);

			// load some input data and expected output data
			string raw_data_string = FileRead.readFileData("X.json");
			double[,] X            = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) JSONNode.Parse(raw_data_string))["X"]
			);
			raw_data_string        = FileRead.readFileData("Y.json");
			double[,] Y            = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) JSONNode.Parse(raw_data_string))["Y"]
			);
			raw_data_string        = "";

			// apply the network to the data
			for(int t = 0; t < 10; t++) {
				double[] y_expected = DenseMatrix.Row(Y, t);
				double[] x          = DenseMatrix.Row(X, t);
				network.Update(x);
				double[] y_actual   = network.GetOutput();
				for(int i = 0; i < y_expected.Length; i++) {
					if(Math.Abs(y_expected[i] - y_actual[i]) > 0.01) {
						System.Console.WriteLine("Error at time :" + t + ": Expected y[" + i + "] = " + y_expected[i] + " but was " + y_actual[i]);
					}
				}
			}
		}*/
	}
}

/*
* C# GMLVQ - Copyright 2016 Benjamin Paaßen
*
* Theoretical Computer Science for Cognitive Systems
* Cognitive Interaction Technology (CITEC)
* Bielefeld University
*
* A simple GMLVQ implementation, only for classification, without training.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;
using EchoState;

namespace GMLVQ {
	public class GMLVQModel {

		// Let K be the number of Prototypes, and m be the number
		// of data dimensions

		// A K x m matrix specifying the prototypes
		public readonly double[,] Prototypes;
		// A K x 1 vector specifying the label for each prototype
		public readonly int[] Prototype_Labels;
		// A m x m matrix morphing the metric used for classification
		public readonly double[,] Lambda;
		public readonly int K;
		public readonly int m;

		// Creates a GMLVQ Model with the given parameters.
		public GMLVQModel(double[,] Prototypes, int[] Prototype_Labels, double[,] Lambda) {
			this.K = Prototypes.GetLength(0);
			this.m = Prototypes.GetLength(1);

			if(K != Prototype_Labels.Length) throw new ArgumentException("Expected one " +
				"label for each prototype, but got " + K + " prototypes and " +
				Prototype_Labels.Length + " labels!");

			if(m != Lambda.GetLength(0) || m != Lambda.GetLength(1))
				throw new ArgumentException("Expected a " + m + "x" + m +
				"matrix Lambda, but got a " + Lambda.GetLength(0) + "x" + Lambda.GetLength(1) +
				"matrix!");

			this.Prototypes       = Prototypes;
			this.Prototype_Labels = Prototype_Labels;
			this.Lambda           = Lambda;
		}

		// Computes the squared distance of the given data point (a m x 1 vector) to all prototypes. The
		// result will be a K x 1 vector containing in each entry k the value of
		// (x - w[k, :])' * Lambda * (x - w[k, :])
		public double[] DistanceToPrototypes(double[] x) {
			double[] d = new double[K];
			for(int k = 0; k < K; k++) {
				// first compute the vector x - w[k, :];
				double[] diff = new double[m];
				for(int i = 0; i < m; i++) {
					diff[i] = x[i] - Prototypes[k, i];
				}
				// then compute the squared distance diff' * Lambda * diff
				for(int i = 0; i < m; i++) {
					double d_tmp = 0;
					for(int j = 0; j < m; j++) {
						d_tmp += Lambda[i, j] * diff[j];
					}
					d[k] += diff[i] * d_tmp;
				}
			}
			return d;
		}

		// Assigns the label of the closest prototype to the given point.
		public int Classify(double[] x) {
			double[] d = DistanceToPrototypes(x);
			int min_k  = 0;
			for(int k = 1; k < K; k++) {
				if(d[k] < d[min_k]) min_k = k;
			}
			return Prototype_Labels[min_k];
		}

		public void Confidence(double[] x, out int label, out double confidence) {
			double[] d = DistanceToPrototypes(x);
			int min_k  = 0;
			double d_minus = double.PositiveInfinity;
			for(int k = 1; k < K; k++) {
				if(d[k] < d[min_k]) {
					min_k = k;
				} else if(d[k] < d_minus) {
					d_minus = d[k];
				}
			}
			confidence = (d_minus - d[min_k]) / (d_minus + d[min_k]);
			label = Prototype_Labels[min_k];
		}

		// Generates an output vector for the given input data point. The output vector has
		// as many entries as there are degrees of freedom (in this case we assume that num_dofs = 2)
		// and the entries are either {-1, 0, 1}.
		//
		// Note that, internally, this classifier assigns a single class to each combination and
		// re-maps the internal class index to vector entries.
		public double[] GetOutput(double[] x) {
			int y = Classify(x);
			switch(y) {
				case 1:
					return new double[]{-1, -1};
				case 2:
					return new double[]{-1, 0};
				case 3:
					return new double[]{-1, 1};
				case 4:
					return new double[]{0, -1};
				case 5:
					return new double[]{0, 0};
				case 6:
					return new double[]{0, 1};
				case 7:
					return new double[]{1, -1};
				case 8:
					return new double[]{1, 0};
				case 9:
					return new double[]{1, 1};
				default:
					throw new ArgumentException("Expected a label in the range [1,9]!");
			}
		}

		// Reads an GMLVQModel from a JSON file.
		public static GMLVQModel FromJSON(string file) {
			string data = FileRead.readFileData(file);
			return FromJSON(
				(JSONClass) ((JSONClass) JSONNode.Parse(data))["gmlvq"]
			);
		}


		// Reads multiple GMLVQModels from a JSON file.
		public static GMLVQModel[] MultiFromJSON(string file) {
			string data = FileRead.readFileData(file);
			return MultiFromJSON(
				(JSONArray) ((JSONClass) JSONNode.Parse(data))["gmlvq"]
			);
		}

		// converts the JSON representation of a GMLVQ Model to a GMLVQModel
		public static GMLVQModel[] MultiFromJSON(JSONArray jGMLVQ) {

			int numModels = jGMLVQ.Count;
			GMLVQModel[] models = new GMLVQModel[numModels];
			for(int m = 0; m < numModels; m++) {
				models[m] = FromJSON((JSONClass) jGMLVQ[m][0]);
			}
			return models;
		}

		// converts the JSON representation of a GMLVQ Model to a GMLVQModel
		public static GMLVQModel FromJSON(JSONClass jGMLVQ) {
			// read prototypes in matrix form
			double[,] Prototypes   = DenseMatrix.FromJSON((JSONArray) jGMLVQ["w"]);
			// read prototype labels in matrix form (column vectors are matrices in json)
			JSONArray jproto_labels = (JSONArray) jGMLVQ["c_w"];
			int[] Prototype_Labels = new int[jproto_labels.Count];
			for(int k = 0; k < Prototype_Labels.Length; k++) {
				Prototype_Labels[k] = ((JSONArray) jproto_labels[k])[0].AsInt;
			}
			double[,] Omega        = DenseMatrix.FromJSON((JSONArray) jGMLVQ["omega"]);
			// Calculate Lambda = Omega * Omega'
			double[,] Lambda       = new double[Omega.GetLength(0), Omega.GetLength(0)];
			for(int i = 0; i < Omega.GetLength(0); i++) {
				for(int j = i; j < Omega.GetLength(0); j++) {
					for(int k = 0; k < Omega.GetLength(1); k++) {
						Lambda[i,j] += Omega[i,k] * Omega[j,k];
					}
					Lambda[j,i] = Lambda[i,j];
				}
			}
			GMLVQModel model       = new GMLVQModel(Prototypes, Prototype_Labels, Lambda);
			return model;
		}
/*
		public void Display() {
			System.Console.WriteLine("======");
			System.Console.WriteLine("GMLVQ Model with " + K + " prototypes and " + m + " dimensions:");
			System.Console.WriteLine("======");
			System.Console.WriteLine("Prototypes:");
			DenseMatrix.Display(Prototypes);
			System.Console.WriteLine("======");
			System.Console.Write("Prototype Labels: ");
			System.Console.Write("[");
			System.Console.Write(Prototype_Labels[0]);
			for(int i = 1; i < Prototype_Labels.Length; i++) {
				System.Console.Write(", ");
				System.Console.Write(Prototype_Labels[i]);
			}
			System.Console.WriteLine("]");
			System.Console.WriteLine("======");
			System.Console.WriteLine("Lambda:");
			DenseMatrix.Display(Lambda);
			System.Console.WriteLine("======");
		}
		// A simple Main Method for testing purposes
		public static void Main() {
			// Load an example model from a JSON file
			GMLVQModel model = FromJSON("gmlvq.json");
			//model.Display();

			// read some test data for the model
			string raw_data_string = FileRead.readFileData("gmlvq_srcdata.json");
			double[,] X_src         = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) ((JSONClass) JSONNode.Parse(raw_data_string))["gmlvq_srcdata"])["X"]
			);
			double[,] Y_src         = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) ((JSONClass) JSONNode.Parse(raw_data_string))["gmlvq_srcdata"])["Y"]
			);

			// check the model prediction; it should be the same as in MATLAB
			for(int t = 0; t < X_src.GetLength(0); t++) {
				// classify point
				int y = model.Classify(DenseMatrix.Row(X_src, t));
				if(y != Math.Round(Y_src[t, 0]))
						System.Console.WriteLine("Error at time :" + t + ": Expected y = " + Y_src[t, 0] + " but was " + y);
			}

			// read the transformation
			raw_data_string = FileRead.readFileData("transform.json");
			double[,] T     = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) JSONNode.Parse(raw_data_string))["transform"]
			);

			// read some transformed test data for the model
			raw_data_string = FileRead.readFileData("gmlvq_tardata.json");
			double[,] X_tar = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) ((JSONClass) JSONNode.Parse(raw_data_string))["gmlvq_tardata"])["X"]
			);
			double[,] Y_tar = DenseMatrix.FromJSON(
				(JSONArray) ((JSONClass) ((JSONClass) JSONNode.Parse(raw_data_string))["gmlvq_tardata"])["Y"]
			);

			// check the model prediction; it should be the same as in MATLAB
			for(int t = 0; t < X_tar.GetLength(0); t++) {
				// classify point
				double[] x_transf = DenseMatrix.multiply(T, DenseMatrix.Row(X_tar, t));
				int y = model.Classify(x_transf);
				if(y != Math.Round(Y_tar[t, 0]))
						System.Console.WriteLine("Error at time :" + t + ": Expected y = " + Y_tar[t, 0] + " but was " + y);
			}
		}
*/
	}
}

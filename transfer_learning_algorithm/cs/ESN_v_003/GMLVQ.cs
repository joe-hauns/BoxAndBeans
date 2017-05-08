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

		// Reads an GMLVQModel from a JSON file.
		public static GMLVQModel FromJSON(string file) {
			string data = readFileData(file);
			return FromJSON(
				(JSONClass) ((JSONClass) JSONNode.Parse(data))["gmlvq"]
			);
		}

		// converts the JSON representation of a GMLVQ Model to a GMLVQModel
		public static GMLVQModel FromJSON(JSONClass jGMLVQ) {
			double[,] Prototypes   = DenseMatrix.FromJSON((JSONArray) jGMLVQ["w"]);
			int[] Prototype_Labels = DenseMatrix.IntVectorFromJSON((JSONArray) jGMLVQ["c_w"]);
			double[,] Lambda       = DenseMatrix.FromJSON((JSONArray) jGMLVQ["lambda"]);
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
			System.Console.WriteLine("[" + String.Join(", ", Prototype_Labels) + "]");
			System.Console.WriteLine("======");
			System.Console.WriteLine("Lambda:");
			DenseMatrix.Display(Lambda);
			System.Console.WriteLine("======");
		}

		// A simple Main Method for testing purposes
		public static void Main() {
			// Load an example model from a JSON file
			GMLVQModel model = FromJSON("example_gmlvq.json");
			model.Display();

			// create some random input data
			int N            = 20;
			double sigma     = 0.2;
			Random rand      = new Random();
			for(int n = 0; n < N; n++) {
				// choose a random class
				int y_expected = rand.Next(-1, 2);
				// generate data around the respective mean
				double[] x     = new double[2];
				x[0]           = y_expected + rand.NextDouble() * sigma;
				x[1]           = rand.NextDouble() * sigma;
				// display the vector
				System.Console.Write("x = ");
				DenseMatrix.DisplayVector(x);
				// display the distances to the prototypes
				double[] d_actual  = model.DistanceToPrototypes(x);
				System.Console.Write("d_actual = ");
				DenseMatrix.DisplayVector(d_actual);
				// display classification
				int y_actual   = model.Classify(x);
				System.Console.WriteLine("y_expected = " + y_expected + " / y_actual = " + y_actual);
			}
		}*/
	}
}

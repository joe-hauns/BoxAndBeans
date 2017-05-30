using System;

public class Filter {
	private double[] buffer;
	private int last;

	public Filter(int size) {
		this.buffer = new double[size];
		this.last = 0;
	}
	 
	public void push(double value) {
		buffer [last++] = value;
		last = last % buffer.Length;
	}

	private double mean() {
		var res = 0.0;
		foreach (double d in buffer) {
			res += d;
		}
		return res / buffer.Length;
	}

	public double get() {
		var mean = this.mean ();
		var x = 0.0;
		foreach (var d in buffer) {
			var diff = d - mean;
			x += diff * diff;
		}
		return Math.Sqrt (x / buffer.Length);
	}
}
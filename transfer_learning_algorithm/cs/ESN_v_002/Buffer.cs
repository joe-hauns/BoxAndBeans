/*
* EchoStateNetwork - Copyright 2016 Benjamin Paaßen
* 
* Theoretical Computer Science for Cognitive Systems
* Cognitive Interaction Technology (CITEC)
* Bielefeld University
*
* A Buffer for storing raw data.
*/

using System;

namespace EchoState {

	public class Buffer {

		// The size of this buffer.
		public readonly int size;
		// The overlap with previous anf following buffer content.
		public readonly int overlap;
		// The number of channels
		public readonly int C;
		// The actual data in this buffer
		private double[,] data;
		// The index pointer in this buffer
		private int b = 0;

		public Buffer(int size, int overlap, int C) {
			this.size    = size;
			this.overlap = overlap;
			this.C       = C;
			this.data    = new double[size + 2*overlap, C];
		}

		public Buffer() : this(8, 8, 8) {}

		// stores the given frame in this buffer.
		// returns true if this buffer is full after storage.
		public bool store(double[] frame) {
			if(isFull()) throw new ArgumentException("Buffer is full!");
			if(frame.Length != C) throw new ArgumentException("Expected " + C + " channels but got a frame with " + frame.Length + " channels.");
			for(int c = 0; c < C; c++) {
				data[b, c] = frame[c];
			}
			b++;
			return isFull();
		}

		// stores the given frame in this buffer.
		// returns true if this buffer is full after storage.
		public bool store(int[] frame) {
			if(isFull()) throw new ArgumentException("Buffer is full!");
			if(frame.Length != C) throw new ArgumentException("Expected " + C + " channels but got a frame with " + frame.Length + " channels.");
			for(int c = 0; c < C; c++) {
				data[b, c] = frame[c];
			}
			b++;
			return isFull();
		}

		// stores the given frame in this buffer.
		public bool isFull() {
			return b == data.GetLength(0);
		}

		// Returns the data contained in this buffer and resets the buffer.
		public double[,] retrieve() {
			// store the old data in a temporary variable
			double[,] oldData = new double[size + 2*overlap, C];
			Array.Copy(this.data, 0, oldData, 0, (size+2*overlap)*C);
			// reset the index
			this.b = 0;
			// override the beginning of the buffer with the overlap data
			for(; this.b < overlap; this.b++) {
				for(int c = 0; c < C; c++) {
					this.data[this.b, c] = oldData[this.b, c];
				}
			}
			// return the old data
			return oldData;
		}	
	}
}

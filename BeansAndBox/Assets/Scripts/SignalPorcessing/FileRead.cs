/*
* EchoStateNetwork - Copyright 2016 Benjamin Paaßen
* 
* Theoretical Computer Science for Cognitive Systems
* Cognitive Interaction Technology (CITEC)
* Bielefeld University
*
* Feature computation for machine learning methods
*/
using System.IO;
using System.Text;

namespace EchoState {
	public class FileRead {

		private FileRead() {}

		// reads all data in a file and puts it into a string
		public static string readFileData(string path) {
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
	}
}

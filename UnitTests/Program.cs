using System;

namespace UnitTests {
	public static class Program {
		
		public static void Main() {
			//Wav2FlacTests tests = new Wav2FlacTests();
			FLACSharpTests tests = new FLACSharpTests();
			tests.Encode();
		}
		
	}
}


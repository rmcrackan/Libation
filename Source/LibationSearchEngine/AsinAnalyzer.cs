using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationSearchEngine
{
	internal class AsinAnalyzer : Analyzer
	{
		public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
		{
			return new AsinFilter(reader);
		}
		/// <summary>
		/// Emits the entire input as a single token and removes
		/// trailing .00 from strings that parsed to numbers
		/// 
		/// Based on Lucene.Net.Analysis.KeywordTokenizer
		/// </summary>
		private class AsinFilter : Tokenizer
		{
			private bool done;
			private int finalOffset;
			private readonly ITermAttribute termAtt;
			private readonly IOffsetAttribute offsetAtt;
			private const int DEFAULT_BUFFER_SIZE = 256;

			public AsinFilter(System.IO.TextReader input) : base(input)
			{
				offsetAtt = AddAttribute<IOffsetAttribute>();
				termAtt = AddAttribute<ITermAttribute>();
				termAtt.ResizeTermBuffer(DEFAULT_BUFFER_SIZE);
			}
			public override bool IncrementToken()
			{
				var charReader = input as CharReader;
				if (!done)
				{
					ClearAttributes();
					done = true;
					int upto = 0;
					char[] buffer = termAtt.TermBuffer();

					while (true)
					{
						int length = charReader.Read(buffer, upto, buffer.Length - upto);
						if (length == 0)
							break;
						upto += length;
						if (upto == buffer.Length)
							buffer = termAtt.ResizeTermBuffer(1 + buffer.Length);
					}

					var termStr = new string(buffer, 0, upto);
					if (termStr.EndsWith(".00"))
						upto -= 3;

					termAtt.SetTermLength(upto);
					finalOffset = CorrectOffset(upto);
					offsetAtt.SetOffset(CorrectOffset(0), finalOffset);
					return true;
				}
				return false;
			}
			public override void End()
			{
				// set final offset 
				offsetAtt.SetOffset(finalOffset, finalOffset);
			}

			public override void Reset(System.IO.TextReader input)
			{
				base.Reset(input);
				this.done = false;
			}
		}
	}
}

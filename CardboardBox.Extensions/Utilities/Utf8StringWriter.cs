﻿using System.Text;

namespace CardboardBox
{
	public class Utf8StringWriter : StringWriter
	{
		public override Encoding Encoding { get { return Encoding.UTF8; } }
	}
}

// 
// TextVisualizer.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Mono.Debugging.Client;
using Gtk;
using MonoDevelop.Core;

namespace MonoDevelop.Debugger.Visualizer
{
	public class TextVisualizer: IValueVisualizer
	{
		const int CHUNK_SIZE = 1024;
		
		TextView textView;
		RawValueString raw;
		ObjectValue val;
		uint idle_id;
		int length;
		int offset;
		
		public TextVisualizer ()
		{
		}
		
		public string Name {
			get {
				return GettextCatalog.GetString ("Text");
			}
		}
		
		public bool CanVisualize (ObjectValue val)
		{
			return val.TypeName == "string";
		}
		
		bool GetNextStringChunk ()
		{
			int amount = Math.Min (length - offset, CHUNK_SIZE);
			string chunk = raw.Substring (offset, amount);
			TextIter iter = textView.Buffer.EndIter;
			
			textView.Buffer.Insert (ref iter, chunk);
			offset += amount;
			
			if (offset < length)
				return true;
			
			idle_id = 0;
			
			// Remove this idle callback
			return false;
		}

		public Widget GetVisualizerWidget (ObjectValue val)
		{
			VBox box = new VBox (false, 6);
			textView = new TextView () { WrapMode = WrapMode.Char };
			ScrolledWindow scrolled = new ScrolledWindow ();
			scrolled.HscrollbarPolicy = PolicyType.Automatic;
			scrolled.VscrollbarPolicy = PolicyType.Automatic;
			scrolled.ShadowType = ShadowType.In;
			scrolled.Add (textView);
			box.PackStart (scrolled, true, true, 0);
			CheckButton cb = new CheckButton (GettextCatalog.GetString ("Wrap text"));
			cb.Active = true;
			cb.Toggled += delegate {
				if (cb.Active)
					textView.WrapMode = WrapMode.Char;
				else
					textView.WrapMode = WrapMode.None;
			};
			box.PackStart (cb, false, false, 0);
			box.ShowAll ();
			
			EvaluationOptions ops = DebuggingService.DebuggerSession.EvaluationOptions.Clone ();
			ops.ChunkRawStrings = true;
			
			raw = val.GetRawValue (ops) as RawValueString;
			length = raw.Length;
			this.val = val;
			offset = 0;
			
			if (length > 0) {
				idle_id = GLib.Idle.Add (GetNextStringChunk);
				textView.Destroyed += delegate {
					if (idle_id != 0) {
						GLib.Source.Remove (idle_id);
						idle_id = 0;
					}
				};
			}
			
			return box;
		}
		
		public bool StoreValue (ObjectValue val)
		{
			val.SetRawValue (textView.Buffer.Text);
			return true;
		}
		
		public bool CanEdit (ObjectValue val)
		{
			return true;
		}
	}
}


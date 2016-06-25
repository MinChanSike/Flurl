﻿using System.Net.Http;
using System.Text;

namespace Flurl.Http.Content
{
	/// <summary>
	/// Provides HTTP content based on a string, with the string itself captured to a property
	/// so it can be read without affecting the read-once content stream.
	/// </summary>
	public class CapturedStringContent : StringContent
	{
		private readonly string _content;

		/// <summary>
		/// The content body captured as a string. Can be read multiple times (unlike the content stream).
		/// </summary>
		public string Content {
			get { return _content; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CapturedStringContent"/> class.
		/// </summary>
		/// <param name="content">The content.</param>
		public CapturedStringContent(string content) : base(content) {
			_content = content;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CapturedStringContent"/> class.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="mediaType">Type of the media.</param>
		public CapturedStringContent(string content, Encoding encoding, string mediaType) : base(content, encoding, mediaType) {
			_content = content;
		}
	}
}
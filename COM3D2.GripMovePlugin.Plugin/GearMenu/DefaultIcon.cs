using System;

namespace GearMenu
{
	internal static class DefaultIcon
	{
		public static byte[] Png
		{
			get
			{
				if (DefaultIcon.png == null)
				{
					DefaultIcon.png = Convert.FromBase64String(DefaultIcon.pngBase64);
				}
				return DefaultIcon.png;
			}
		}

		private static byte[] png = null;

		private static string pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAA3NCSVQICAjb4U/gAAAACXBIWXMAABYlAAAWJQFJUiTwAAAA/0lEQVRIie2WPYqFMBRGb35QiARM4QZSuAX3X7sDkWwgRYSQgJLEKfLGh6+bZywG/JrbnZPLJfChfd/hzuBb6QBA89i2zTlnjFmWZV1XAPjrZgghAKjrum1bIUTTNFVVvQXOOaXUNE0xxhDC9++llBDS972U8iTQWs/zPAyDlPJreo5SahxHzrkQAo4baK0B4Dr9gGTgW4Ax5pxfp+dwzjH+JefhvaeUlhJQSr33J0GMsRT9A3j7P3gEj+ARPIJHUFBACCnLPYAvAWPsSpn4SAiBMXYSpJSstaUE1tqU0knQdR0AKKWu0zMkAwEA5QZnjClevHIvegnuq47o37frH81sg91rI7H3AAAAAElFTkSuQmCC";
	}
}

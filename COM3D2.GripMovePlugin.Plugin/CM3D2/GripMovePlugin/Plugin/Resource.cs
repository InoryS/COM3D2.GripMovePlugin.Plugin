using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CM3D2.GripMovePlugin.Plugin
{
	// Token: 0x02000023 RID: 35
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resource
	{
		// Token: 0x06000127 RID: 295 RVA: 0x000024C9 File Offset: 0x000006C9
		internal Resource()
		{
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000128 RID: 296 RVA: 0x00002E81 File Offset: 0x00001081
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resource.resourceMan == null)
				{
					Resource.resourceMan = new ResourceManager("CM3D2.GripMovePlugin.Plugin.Resource", typeof(Resource).Assembly);
				}
				return Resource.resourceMan;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000129 RID: 297 RVA: 0x00002EAD File Offset: 0x000010AD
		// (set) Token: 0x0600012A RID: 298 RVA: 0x00002EB4 File Offset: 0x000010B4
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resource.resourceCulture;
			}
			set
			{
				Resource.resourceCulture = value;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x0600012B RID: 299 RVA: 0x00002EBC File Offset: 0x000010BC
		internal static byte[] gripmovepluginshaders
		{
			get
			{
				return (byte[])Resource.ResourceManager.GetObject("gripmovepluginshaders", Resource.resourceCulture);
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600012C RID: 300 RVA: 0x00002ED7 File Offset: 0x000010D7
		internal static byte[] mouse_cursor
		{
			get
			{
				return (byte[])Resource.ResourceManager.GetObject("mouse_cursor", Resource.resourceCulture);
			}
		}

		// Token: 0x040000CA RID: 202
		private static ResourceManager resourceMan;

		// Token: 0x040000CB RID: 203
		private static CultureInfo resourceCulture;
	}
}

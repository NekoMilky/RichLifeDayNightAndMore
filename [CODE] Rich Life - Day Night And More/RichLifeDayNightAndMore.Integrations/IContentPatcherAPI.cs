using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace RichLifeDayNightAndMore.Integrations;

public interface IContentPatcherAPI
{
	void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>?> getValue);
}

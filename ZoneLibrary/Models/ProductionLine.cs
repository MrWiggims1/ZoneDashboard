using System.ComponentModel.DataAnnotations;

namespace ZoneProductionLibrary.Models;

public enum ProductionLine
{
	[Display(Description = "Gen 2")] Gen2,
	[Display(Description = "Expedition")] Expo
}
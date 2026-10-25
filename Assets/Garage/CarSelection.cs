using System.Linq;
using UnityEngine;

public static class CarSelection
{
    public const string PreferenceKey = "SelectedCar";
    static CarDefinition[] cars;
    public static CarDefinition[] All => cars ?? (cars = Resources.LoadAll<CarDefinition>("Cars").OrderBy(c => c.order).ToArray());
    public static CarDefinition Current => All.FirstOrDefault(c => c.id == PlayerPrefs.GetString(PreferenceKey, "sports"))
        ?? All.First(c => c.id == "sports");
    public static void Select(CarDefinition car)
    {
        PlayerPrefs.SetString(PreferenceKey, car.id);
        PlayerPrefs.Save();
    }
}

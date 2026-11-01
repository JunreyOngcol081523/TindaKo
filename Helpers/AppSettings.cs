namespace TindaKo.Helpers;

/// <summary>
/// Single place for the store settings keys. Other ViewModels (Sell, Buyer, Creditors...)
/// should read AppSettings.AllowUtang / DueDateDays instead of repeating string keys.
/// The key names are the same ones already saved on users' devices.
/// </summary>
public static class AppSettings
{
    private const string StoreNameKey = "StoreName";
    private const string StoreAddressKey = "StoreAddress";
    private const string StoreEmailKey = "StoreEmail";
    private const string StorePhoneNumKey = "StorePhoneNum";
    private const string AllowUtangKey = "AllowUtang";
    private const string DueDateDaysKey = "DueDateDays";

    public static string StoreName
    {
        get => Preferences.Get(StoreNameKey, string.Empty);
        set => Preferences.Set(StoreNameKey, value ?? string.Empty);
    }

    public static string StoreAddress
    {
        get => Preferences.Get(StoreAddressKey, string.Empty);
        set => Preferences.Set(StoreAddressKey, value ?? string.Empty);
    }

    public static string StoreEmail
    {
        get => Preferences.Get(StoreEmailKey, string.Empty);
        set => Preferences.Set(StoreEmailKey, value ?? string.Empty);
    }

    public static string StorePhoneNum
    {
        get => Preferences.Get(StorePhoneNumKey, string.Empty);
        set => Preferences.Set(StorePhoneNumKey, value ?? string.Empty);
    }

    public static bool AllowUtang
    {
        get => Preferences.Get(AllowUtangKey, false);
        set => Preferences.Set(AllowUtangKey, value);
    }

    public static int DueDateDays
    {
        get => Preferences.Get(DueDateDaysKey, 0);
        set => Preferences.Set(DueDateDaysKey, value);
    }
}

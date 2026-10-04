using System.Globalization;

namespace SimpleWallet.Localization;

public interface ITextLocalizer
{
    string this[string key] { get; }
}

public class TextLocalizer : ITextLocalizer
{
    public string this[string key] => Texts.Translate(key, CultureInfo.CurrentUICulture.Name);
}

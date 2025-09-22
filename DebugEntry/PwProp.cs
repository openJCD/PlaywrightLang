using PlaywrightLang.LanguageServices;

namespace PlaywrightLang.DebugEntry;

public class PwProp : PwObjectClass
{
    [PwItem("state")]
    public float ItemState;

    [PwItem("dispenses_item")]
    public string GiveItem()
    {
        return "test item";
    }
}
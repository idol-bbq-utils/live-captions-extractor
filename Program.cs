using System.Windows.Automation;

class Program
{
    static void Main(string[] args)
    {
        string windowTitle;
        if (args.Length == 0)
        {
            windowTitle = "实时辅助字幕";
            Console.WriteLine("Usage: win11-subtitle-extractor.exe \"window title\"");
        }
        else
        {
            windowTitle = args[0];
        }

        AutomationElement window = FindWindowByTitle(windowTitle);

        if(window != null){
            List<string> texts = new List<string>();
            WalkAutomationTreeAndCollectTexts(window, texts);
            foreach (string text in texts)
            {
                Console.WriteLine(text);
            }
        }
        else
        {
            Console.WriteLine("Window not found");
        }
    }

    static AutomationElement FindWindowByTitle(string title)
    {
        var condition = new PropertyCondition(AutomationElement.NameProperty, title);
        return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
    }

    static void WalkAutomationTree(AutomationElement rootElement)
    {
        var condition = Condition.TrueCondition;
        var treeWalker = new TreeWalker(condition);
        WalkAutomationTree(treeWalker, rootElement, 0);
    }

    static void WalkAutomationTree(TreeWalker walker, AutomationElement element, int indent)
    {

        string indentString = new string(' ', indent);
        string elementText = GetTextFromElement(element);
        Console.WriteLine($"{indentString}{element.Current.ControlType.ProgrammaticName} - Name: {element.Current.Name}, Text:{elementText}");
        AutomationElement child = walker.GetFirstChild(element);
        while (child != null)
        {
            WalkAutomationTree(walker, child, indent + 2);
            child = walker.GetNextSibling(child);
        }
    }

    static string GetTextFromElement(AutomationElement element)
    {
        try
        {
            object patternObj;
            if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                var textPattern = (TextPattern)patternObj;
                return textPattern.DocumentRange.GetText(-1).TrimEnd('\r');
            }
            else if (element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
            {
                var valuePattern = (ValuePattern)patternObj;
                return valuePattern.Current.Value;
            }
            else
            {
                return string.Empty;
            }
        } catch (ElementNotAvailableException)
        {
            return string.Empty;
        }
    }

    static void WalkAutomationTreeAndCollectTexts(AutomationElement element, List<string> texts)
    {
        var treeWalker = TreeWalker.RawViewWalker;
        CollectTextsRecursively(treeWalker, element, texts);
    }
    
    static void CollectTextsRecursively(TreeWalker walker, AutomationElement element, List<string> texts)
    {
        string elementText = GetTextFromElement(element);
        if (!string.IsNullOrEmpty(elementText))
        {
            texts.Add(elementText);
        }
        AutomationElement child = walker.GetFirstChild(element);
        while (child != null)
        {
            CollectTextsRecursively(walker, child, texts);
            child = walker.GetNextSibling(child);
        }
    }
}
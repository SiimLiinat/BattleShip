using System;
using System.Collections.Generic;
using System.Linq;

namespace MenuSystem
{
    public enum MenuLevel
    {
        Level0,
        Level1,
        Level2Plus,
    }
    
    public class Menu
    {
        public bool BreakMenu = false;

        private Dictionary<string, MenuItem> MenuItems { get; set; } = new();
        private readonly MenuLevel _menuLevel;
        private readonly int _lines;

        public Menu(MenuLevel level, int? lines = 1)
        {
            _menuLevel = level;
            if (lines != null) _lines = lines.Value;
        }

        public string RunMenu()
        {
            string userChoice = "";
            do
            {
                var items = MenuItems.Values.ToArray();
                var itemIndex = ConsoleHelper.MultipleChoice(true, items, _lines);
                if (itemIndex == -1)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Closing down...");
                    return "";
                }

                userChoice = MenuItems.FirstOrDefault(x => x.Value == items[itemIndex]).Key;

                Console.WriteLine("");
                Console.WriteLine("");

                var userChoiceFromAction = "";
                if (MenuItems.TryGetValue(userChoice, out MenuItem? userMenuItem))
                {
                    userChoiceFromAction = userMenuItem.MethodToExecute();
                }
                else
                {
                    Console.WriteLine("I don't have this option.");
                }

                if (userChoiceFromAction.Length > 0) userChoice = userChoiceFromAction;
            } while (userChoice != "X" && userChoice != "M" && userChoice != "P" && BreakMenu == false);
            
            if (_menuLevel == MenuLevel.Level1 && userChoice == "M") return "";
            return userChoice;
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            if (menuItem.UserChoice == "") throw new ArgumentException("UserChoice cannot be empty!");
            if (!MenuItems.Keys.Contains(menuItem.UserChoice)) MenuItems.Add(menuItem.UserChoice, menuItem);
        }
        
        public void RemoveMenuItem(string userChoice)
        {
            if (MenuItems.Keys.Contains(userChoice)) MenuItems.Remove(userChoice);
        }

        public void ClearMenuItems()
        {
            MenuItems.Clear();
        }
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EventManagmentApp
{
    public static class SizeManager
    {
        private static string _currentSize = "Medium";

        public static string CurrentSize
        {
            get => _currentSize;
            private set
            {
                if (_currentSize != value)
                {
                    _currentSize = value;
                    OnStaticPropertyChanged();

                    // Уведомляем об изменении зависимых свойств
                    OnStaticPropertyChanged(nameof(IsSmallSize));
                    OnStaticPropertyChanged(nameof(IsMediumSize));
                    OnStaticPropertyChanged(nameof(IsLargeSize));
                }
            }
        }

        public static bool IsSmallSize => CurrentSize == "Small";
        public static bool IsMediumSize => CurrentSize == "Medium";
        public static bool IsLargeSize => CurrentSize == "Large";

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public static void ApplySize(string size)
        {
            CurrentSize = size;
            ApplySizeToApplication();
        }

        public static void ApplySizeToApplication()
        {
            var resources = Application.Current.Resources;

            switch (CurrentSize)
            {
                case "Small":
                    ApplySmallSize(resources);
                    break;
                case "Medium":
                    ApplyMediumSize(resources);
                    break;
                case "Large":
                    ApplyLargeSize(resources);
                    break;
            }
        }

        private static void ApplySmallSize(ResourceDictionary resources)
        {
            // Иконки
            resources["DefaultIconWidth"] = (double)resources["SmallIconWidth"];
            resources["DefaultIconHeight"] = (double)resources["SmallIconHeight"];
            resources["ButtonIconWidth"] = (double)resources["SmallButtonIconWidth"];
            resources["ButtonIconHeight"] = (double)resources["SmallButtonIconHeight"];

            // Шрифты
            resources["AppFontSizeBig"] = (double)resources["SmallAppFontSizeBig"];
            resources["AppFontSizeH1"] = (double)resources["SmallAppFontSizeH1"];
            resources["AppFontSizeH2"] = (double)resources["SmallAppFontSizeH2"];
            resources["AppFontSizeH3"] = (double)resources["SmallAppFontSizeH3"];
            resources["AppFontSizeH4"] = (double)resources["SmallAppFontSizeH4"];
            resources["WindowFormFontSize"] = (double)resources["SmallWindowFormFontSize"];
        }

        private static void ApplyMediumSize(ResourceDictionary resources)
        {
            // Иконки
            resources["DefaultIconWidth"] = (double)resources["MediumIconWidth"];
            resources["DefaultIconHeight"] = (double)resources["MediumIconHeight"];
            resources["ButtonIconWidth"] = (double)resources["MediumButtonIconWidth"];
            resources["ButtonIconHeight"] = (double)resources["MediumButtonIconHeight"];

            // Шрифты
            resources["AppFontSizeBig"] = (double)resources["MediumAppFontSizeBig"];
            resources["AppFontSizeH1"] = (double)resources["MediumAppFontSizeH1"];
            resources["AppFontSizeH2"] = (double)resources["MediumAppFontSizeH2"];
            resources["AppFontSizeH3"] = (double)resources["MediumAppFontSizeH3"];
            resources["AppFontSizeH4"] = (double)resources["MediumAppFontSizeH4"];
            resources["WindowFormFontSize"] = (double)resources["MediumWindowFormFontSize"];
        }

        private static void ApplyLargeSize(ResourceDictionary resources)
        {
            // Иконки
            resources["DefaultIconWidth"] = (double)resources["LargeIconWidth"];
            resources["DefaultIconHeight"] = (double)resources["LargeIconHeight"];
            resources["ButtonIconWidth"] = (double)resources["LargeButtonIconWidth"];
            resources["ButtonIconHeight"] = (double)resources["LargeButtonIconHeight"];

            // Шрифты
            resources["AppFontSizeBig"] = (double)resources["LargeAppFontSizeBig"];
            resources["AppFontSizeH1"] = (double)resources["LargeAppFontSizeH1"];
            resources["AppFontSizeH2"] = (double)resources["LargeAppFontSizeH2"];
            resources["AppFontSizeH3"] = (double)resources["LargeAppFontSizeH3"];
            resources["AppFontSizeH4"] = (double)resources["LargeAppFontSizeH4"];
            resources["WindowFormFontSize"] = (double)resources["LargeWindowFormFontSize"];
        }

        public static void InitializeSize()
        {
            ApplySizeToApplication();
        }
    }
}
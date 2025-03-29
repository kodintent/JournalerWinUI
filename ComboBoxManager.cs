using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace JournalerWinUI
{
    internal class ComboBoxManager
    {
        private readonly ComboBox _comboBox;
        private readonly string _itemsKey;
        private readonly string _selectedKey;
        private readonly ObservableCollection<string> _items;

        public ComboBoxManager(ComboBox comboBox, string itemsKey, string selectedKey)
        {
            Debug.WriteLine($"ComboBoxManager constructor for {itemsKey}");
            _comboBox = comboBox ?? throw new ArgumentNullException(nameof(comboBox));
            _itemsKey = itemsKey ?? throw new ArgumentNullException(nameof(itemsKey));
            _selectedKey = selectedKey ?? throw new ArgumentNullException(nameof(selectedKey));
            _items = new ObservableCollection<string>();

            // Delay LoadItems to comboBox.Loaded
            _comboBox.Loaded += (s, e) =>
            {
                Debug.WriteLine($"ComboBox {_itemsKey} Loaded");
                LoadItems();
                SetupEvents();
            };
        }

        public void LoadItems()
        {
            Debug.WriteLine($"ComboBoxManager loading items for {_itemsKey}");
            var itemsList = LocalSettings.GetStringListFromLocalsettings(_itemsKey);
            _items.Clear();
            foreach (var item in itemsList)
            {
                _items.Add(item);
            }
            Debug.WriteLine($"Before ItemsSource, _comboBox: {_comboBox != null}, Name: {_comboBox?.Name}");
            _comboBox.ItemsSource = _items;

            string selectedItem = LocalSettings.GetStringFromLocalsettings(_selectedKey) ?? "";
            if (!string.IsNullOrEmpty(selectedItem))
            {
                if (_items.Contains(selectedItem))
                {
                    _comboBox.Text = selectedItem;
                }
                else
                {
                    AddItem(selectedItem);
                }
            }
            Debug.WriteLine($"ComboBoxManager loaded items for {_itemsKey}: {string.Join(", ", _items)}");
        }

        public void AddItem(string text)
        {
            if (!string.IsNullOrEmpty(text) && !_items.Contains(text))
            {
                _items.Add(text);
                LocalSettings.PutStringListInLocalsettings(_itemsKey, new List<string>(_items));
                LocalSettings.PutStringInLocalsettings(_selectedKey, text);
                _comboBox.Text = text; // Set Text instead of SelectedItem
                Debug.WriteLine($"Added item '{text}' to {_itemsKey}");
            }
        }

        public void RemoveItem(string item)
        {
            if (_items.Remove(item))
            {
                LocalSettings.PutStringListInLocalsettings(_itemsKey, new List<string>(_items));
                if (_comboBox.Text == item)
                {
                    _comboBox.Text = "";
                }
                Debug.WriteLine($"Removed item '{item}' from {_itemsKey}");
            }
        }

        private void SetupEvents()
        {
            _comboBox.SelectionChanged += (s, e) =>
            {
                if (_comboBox.SelectedItem is string selectedItem)
                {
                    LocalSettings.PutStringInLocalsettings(_selectedKey, selectedItem);
                    Debug.WriteLine($"Selection changed to '{selectedItem}' for {_selectedKey}");
                }
            };

            _comboBox.TextSubmitted += (s, args) =>
            {
                AddItem(args.Text);
            };
        }

        public ObservableCollection<string> Items => _items;
    }
}
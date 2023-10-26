using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Principal;
using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SaveFormState {
    public class WPF {
        public static void SaveFrameCurrentSource(Page page, string pageName, string filename) {
            FrameState state;
            if (File.Exists(filename)) state = JsonConvert.DeserializeObject<FrameState>(File.ReadAllText(filename));
            else state = new FrameState();

            state.SelectedPage = pageName;
            if (state.Pages == null)
                state.Pages = new();

            if (!state.Pages.ContainsKey(pageName)) {
                state.Pages.Add(pageName, GetControls(page, 0));
            }
            else {
                state.Pages[pageName] = GetControls(page, 0);
            }

            string sJson = JsonConvert.SerializeObject(state);
            File.WriteAllText(filename, sJson);
        }
        public static Dictionary<string, FrameState.Control> GetControls(DependencyObject depObj, int recursion_depth) {
            Dictionary<string, FrameState.Control> controls = new();

            if (recursion_depth > 20) return controls;

            if (depObj == null) throw new Exception("Value is null.");
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null) continue;

                FrameState.Control? control = ConvertDependencyToControl(ithChild);
                if (control != null && !string.IsNullOrEmpty(control.Name)) {
                    controls.Add(control.Name, control);
                }

                // return result from recursive call
                foreach (var childOfChild in GetControls(ithChild, recursion_depth + 1)) controls.Add(childOfChild.Key, childOfChild.Value);
            }

            return controls;
        }
        public static FrameState.Control? ConvertDependencyToControl(DependencyObject depObj) {
            if (depObj == null) return null;

            switch (depObj) {
                case TextBox:
                    TextBox textBox = (TextBox)depObj;
                    return new FrameState.Control() { Name = textBox.Name, Text = textBox.Text };
                case TextBlock:
                    TextBlock textBlock = (TextBlock)depObj;
                    return new FrameState.Control() { Name = textBlock.Name, Text = textBlock.Text }; ;
                case RadioButton:
                    RadioButton radioButton = (RadioButton)depObj;
                    return new FrameState.Control() { Name = radioButton.Name, IsChecked = radioButton.IsChecked }; ;
            }

            return null;
        }

        public static void LoadFrame(Page page, string pageName, string filename) {
            if (!File.Exists(filename)) return;

            string sJson = File.ReadAllText(filename);
            FrameState state = JsonConvert.DeserializeObject<FrameState>(sJson) ?? new();

            // if this page isn't saved, skip
            if (!state.Pages.ContainsKey(pageName)) return;

            SetControls(page, state, pageName);
        }
        public static void SetControls(DependencyObject depObj, FrameState state, string pageName) {

            if (depObj == null) throw new Exception("Value is null.");
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null) continue;

                SetControl(ithChild, state, pageName);

                // Set Nested controls
                SetControls(ithChild, state, pageName);
            }
        }
        public static void SetControl(DependencyObject depObj, FrameState state, string pageName) {
            switch (depObj) {
                case TextBox:
                    TextBox textBox = (TextBox)depObj;
                    if (state.Pages[pageName].ContainsKey(textBox.Name))
                        textBox.Text = state.Pages[pageName][textBox.Name].Text;
                    break;
                case TextBlock:
                    TextBlock textBlock = (TextBlock)depObj;
                    if (state.Pages[pageName].ContainsKey(textBlock.Name))
                        textBlock.Text = state.Pages[pageName][textBlock.Name].Text;
                    break;
                case RadioButton:
                    RadioButton radioButton = (RadioButton)depObj;
                    if (state.Pages[pageName].ContainsKey(radioButton.Name))
                        radioButton.IsChecked = state.Pages[pageName][radioButton.Name].IsChecked;
                    break;
            }
        }
    }


    public class FrameState {
        public string SelectedPage { get; set; } = string.Empty;
        public Dictionary<string, Dictionary<string, Control>> Pages { get; set; }

        public class Control {
            public string Name { get; set; }
            public string ControlType { get; set; }

            public string Content { get; set; }
            public string Text { get; set; }
            public bool? IsChecked { get; set; }
            public int value { get; set; }
            public object Tag { get; set; }
        }
    }
}
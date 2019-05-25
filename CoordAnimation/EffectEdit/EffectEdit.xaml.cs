using BenLib.Standard;
using Coord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoordAnimation
{
    /// <summary>
    /// Logique d'interaction pour EffectEdit.xaml
    /// </summary>
    public partial class EffectEdit : Window
    {
        public CharacterEffect CharacterEffect { get; }
        public Dictionary<CharacterEffectPropertyMetadata, UIElement> Editors { get; }

        public EffectEdit(CharacterEffect characterEffect, Scene scene)
        {
            CharacterEffect = characterEffect;
            Editors = CharacterEffectSerializers[characterEffect.GetType()].ToDictionary(m => m, m => GetPropertyEditor(m, scene));
            InitializeComponent();
            foreach (var kvp in Editors)
            {
                properties.Children.Add(new TextBlock { Text = kvp.Key.DisplayedName });
                properties.Children.Add(kvp.Value);
            }
        }

        public UIElement GetPropertyEditor(CharacterEffectPropertyMetadata metadata, Scene scene)
        {
            var type = metadata.PropertyType;
            return
                type == typeof(string) ? new TextBox { Text = CharacterEffect.GetPropValue(metadata.PropertyName) as string } :
                type == typeof(bool) ? new CheckBox { IsChecked = (bool)CharacterEffect.GetPropValue(metadata.PropertyName) } :
                type == typeof(Interval<int>) ? new IntervalEditor(IntervalType.Int, CharacterEffect.GetPropValue(metadata.PropertyName)) :
                type == typeof(VisualObject) ? (UIElement)new VisualObjectSelector { VisualObjects = new SelectedVisualObjectsCollection(scene.configuration.plane.VisualObjects) } :
                throw new NotImplementedException();
        }

        public static Dictionary<Type, CharacterEffectPropertyMetadata[]> CharacterEffectSerializers = new Dictionary<Type, CharacterEffectPropertyMetadata[]>
        {
            {
                typeof(TranslateCharacterEffect), new[]
                {
                    new CharacterEffectPropertyMetadata("Vector", "Vector", typeof(Vector)),
                    new CharacterEffectPropertyMetadata("In", "In", typeof(bool)),
                }
            },
            {
                typeof(InsertCharacterEffect), new[]
                {
                    new CharacterEffectPropertyMetadata("BoundsInterval", "BoundsInterval", typeof(Interval<int>)),
                    new CharacterEffectPropertyMetadata("VisualObject", "VisualObject", typeof(VisualObject)),
                }
            }
        };

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var kvp in Editors)
            {
                var type = kvp.Key.PropertyType;
                if (type == typeof(string)) CharacterEffect.SetPropValue(kvp.Key.PropertyName, ((TextBox)kvp.Value).Text);
                else if (type == typeof(bool)) CharacterEffect.SetPropValue(kvp.Key.PropertyName, ((CheckBox)kvp.Value).IsChecked.Value);
                else if (type == typeof(Interval<int>)) CharacterEffect.SetPropValue(kvp.Key.PropertyName, ((IntervalEditor)kvp.Value).IntInterval);
                else if (type == typeof(VisualObject)) CharacterEffect.SetPropValue(kvp.Key.PropertyName, ((VisualObjectSelector)kvp.Value).Result);
            }
        }
    }

    public readonly struct CharacterEffectPropertyMetadata
    {
        public CharacterEffectPropertyMetadata(string displayedName, string propertyName, Type propertyType) : this()
        {
            DisplayedName = displayedName;
            PropertyName = propertyName;
            PropertyType = propertyType;
        }

        public string DisplayedName { get; }
        public string PropertyName { get; }
        public Type PropertyType { get; }
    }
}

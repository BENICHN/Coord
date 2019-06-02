using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WpfMath;

namespace Coord
{
    /// <summary>
    /// Définit un <see cref="TextVisualObjectBase"/> à partir d'une chaîne de caractères au format LaTex ou non
    /// </summary>
    public class TextVisualObject : TextVisualObjectBase, ICoordEditable
    {
        public override string Type => (In, LaTex) switch
        {
            (true, true) => "InTex",
            (true, false) => "InText",
            (false, true) => "OutTex",
            (false, false) => "OutText"
        };

        IEnumerable<(string Description, DependencyProperty Property)> ICoordEditable.Properties
        {
            get
            {
                yield return ("Text", TextProperty);
                yield return ("Scale", ScaleProperty);
                yield return ("LaTex", LaTexProperty);
                yield return ("Typeface", TypefaceProperty);
                yield return ("InAnchorPoint", InAnchorPointProperty);
                yield return ("In", InProperty);
                yield return ("RectPoint", RectPointProperty);
            }
        }

        private readonly TexFormulaParser m_texFormulaParser = new TexFormulaParser();

        /// <summary>
        /// Chaîne de caractères à dessiner
        /// </summary>
        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public static readonly DependencyProperty TextProperty = CreateProperty<string>(true, true, "Text", typeof(TextVisualObject));

        /// <summary>
        /// Taille du texte
        /// </summary>
        public double Scale { get => (double)GetValue(ScaleProperty); set => SetValue(ScaleProperty, value); }
        public static readonly DependencyProperty ScaleProperty = CreateProperty<double>(true, true, "Scale", typeof(TextVisualObject));

        /// <summary>
        /// Indique si <see cref="Text"/> est à interpréter comme une chaîne au format LaTex
        /// </summary>
        public bool LaTex { get => (bool)GetValue(LaTexProperty); set => SetValue(LaTexProperty, value); }
        public static readonly DependencyProperty LaTexProperty = CreateProperty<bool>(true, true, "LaTex", typeof(TextVisualObject));

        /// <summary>
        /// Dans le cas où <see cref="LaTex"/> est <see langword="false"/>, décrit le style du texte
        /// </summary>
        public Typeface Typeface { get => (Typeface)GetValue(TypefaceProperty); set => SetValue(TypefaceProperty, value); }
        public static readonly DependencyProperty TypefaceProperty = CreateProperty(true, true, "Typeface", typeof(TextVisualObject), new Typeface("Calibri"));

        /// <summary>
        /// Appelle <see cref="GetOutGeometry(string)"/> en passant <see cref="Text"/> comme paramètre puis retourne le résultat
        /// </summary>
        /// <returns>La <see cref="Geometry"/> représentant le texte mis en forme</returns>
        public Geometry GetOutGeometry() => GetOutGeometry(Text);

        /// <summary>
        /// Obtient une <see cref="Geometry"/> qui représente du texte mis en forme
        /// </summary>
        /// <param name="text">Texte à mettre en forme</param>
        /// <returns>La <see cref="Geometry"/> représentant le texte mis en forme</returns>
        public Geometry GetOutGeometry(string text) => LaTex ? m_texFormulaParser.Parse(text).GetRenderer(TexStyle.Display, Scale, "Cambria Math").RenderToGeometry(0.0, 0.0) : new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, Scale, Brushes.White, 1).BuildGeometry(new Point(0.0, 0.0));

        /// <summary>
        /// Obtient un tableau de <see cref="Character"/> à partir de <see cref="GetOutGeometry"/>
        /// </summary>
        /// <returns>Tableau contenant les <see cref="Character"/> générés de <see cref="GetOutGeometry"/></returns>
        protected override Character[] GetCharactersCore() => GetOutGeometry().ToCharacters().ToArray();

        /// <summary>
        /// Obtient les dimensions d'une <see cref="Geometry"/> qui représente du texte mis en forme
        /// </summary>
        /// <param name="text">Texte à mettre en forme</param>
        /// <returns>Dimensions de la <see cref="Geometry"/> qui représente du texte mis en forme</returns>
        public Size Size(string text) => GetOutGeometry(text).Bounds.Size;
        public Size SizeX(string text) => new Size(Size(text).Width, 0);
        public Size SizeY(string text) => new Size(0, Size(text).Height);

        public Size SizeDiff(string text1, string text2)
        {
            var s1 = Size(text1);
            var s2 = Size(text2);
            return new Size(s1.Width - s2.Width, s1.Height - s2.Height);
        }
        public Vector SizeXDiff(string text1, string text2) => new Vector(Size(text1).Width - Size(text2).Width, 0);
        public Vector SizeYDiff(string text1, string text2) => new Vector(0, Size(text1).Height - Size(text2).Height);

        protected override void OnChanged()
        {
            InCache = null;
            base.OnChanged();
        }

        public override string ToString() => $"{Type} : {Text}";
    }
}

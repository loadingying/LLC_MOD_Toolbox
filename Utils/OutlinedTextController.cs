using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MediaBrush = System.Windows.Media.Brush;
using MediaFontFamily = System.Windows.Media.FontFamily;
using MediaFontStyle = System.Windows.FontStyle;
using MediaSize = System.Windows.Size;

namespace LLC_MOD_Toolbox
{
    public class OutlinedTextControl : FrameworkElement
    {
        // 定义依赖属性
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(OutlinedTextControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(MediaBrush), typeof(OutlinedTextControl),
                new FrameworkPropertyMetadata(System.Windows.Media.Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(OutlinedTextControl),
                new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(MediaBrush), typeof(OutlinedTextControl),
                new FrameworkPropertyMetadata(System.Windows.Media.Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(OutlinedTextControl),
                new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(typeof(OutlinedTextControl),
                new FrameworkPropertyMetadata(System.Windows.SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(typeof(OutlinedTextControl),
                new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontStyleProperty =
            TextElement.FontStyleProperty.AddOwner(typeof(OutlinedTextControl),
                new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TextAlignmentProperty =
        TextBlock.TextAlignmentProperty.AddOwner(
            typeof(OutlinedTextControl),
            new FrameworkPropertyMetadata(
                TextAlignment.Left,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure
            )
        );

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (string.IsNullOrEmpty(Text))
                return;

            var formattedText = CreateFormattedText();
            double textWidth = formattedText.Width;
            double originX = 0;

            // 计算水平绘制原点
            switch (TextAlignment)
            {
                case TextAlignment.Right:
                    originX = RenderSize.Width - textWidth;
                    break;
                case TextAlignment.Center:
                    originX = (RenderSize.Width - textWidth) / 2;
                    break;
                case TextAlignment.Left:
                default:
                    originX = 0;
                    break;
            }

            // 生成几何图形并绘制
            var geometry = formattedText.BuildGeometry(new System.Windows.Point(originX, 0));
            drawingContext.DrawGeometry(Stroke, new System.Windows.Media.Pen(Stroke, StrokeThickness), geometry); // 描边
            drawingContext.DrawGeometry(Fill, null, geometry); // 填充
        }
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public MediaBrush Stroke
        {
            get { return (MediaBrush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public MediaBrush Fill
        {
            get { return (MediaBrush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public MediaFontFamily FontFamily
        {
            get { return (MediaFontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public MediaFontStyle FontStyle
        {
            get { return (MediaFontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        protected override MediaSize MeasureOverride(MediaSize availableSize)
        {
            if (string.IsNullOrEmpty(Text))
                return new MediaSize(0, 0);

            var formattedText = CreateFormattedText();
            return new MediaSize(formattedText.Width, formattedText.Height);
        }

        private FormattedText CreateFormattedText()
        {
            return new FormattedText(
                Text,
                CultureInfo.CurrentUICulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
                FontSize,
                System.Windows.Media.Brushes.Black, // 此处的颜色并不影响渲染，因为使用Geometry绘制
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }
    }

}

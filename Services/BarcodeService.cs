// Services/BarcodeService.cs
using System.Text;
using QRCoder;

namespace Break_Bulk_System.Services
{
    public interface IBarcodeService
    {
        /// <summary>
        /// Renders <paramref name="data"/> as a self-contained Code 128 (subset B) barcode SVG.
        /// Suitable for embedding directly in a page and scanning with a handheld/laser scanner.
        /// <paramref name="label"/> is the human-readable text drawn under the bars; when null
        /// the encoded <paramref name="data"/> is shown. This lets the bars encode something long
        /// (e.g. a URL) while the caption stays short and readable (e.g. the product code).
        /// </summary>
        string GetCode128Svg(string data, int moduleWidth = 2, int barHeight = 80, string? label = null);

        /// <summary>
        /// Renders <paramref name="data"/> as a QR code and returns it as a base64 PNG data URI
        /// (e.g. for the src of an img tag). Ideal for scanning with a phone or tablet camera.
        /// </summary>
        string GetQrCodePngDataUri(string data, int pixelsPerModule = 8);
    }

    public class BarcodeService : IBarcodeService
    {
        // Code 128 module-width patterns for values 0..106.
        // Each string is 6 (or 7 for the stop) widths: bar, space, bar, space, ...
        private static readonly string[] Patterns =
        {
            "212222","222122","222221","121223","121322","131222","122213","122312","132212","221213",
            "221312","231212","112232","122132","122231","113222","123122","123221","223211","221132",
            "221231","213212","223112","312131","311222","321122","321221","312212","322112","322211",
            "212123","212321","232121","111323","131123","131321","112313","132113","132311","211313",
            "231113","231311","112133","112331","132131","113123","113321","133121","313121","211331",
            "231131","213113","213311","213131","311123","311321","331121","312113","312311","332111",
            "314111","221411","431111","111224","111422","121124","121421","141122","141221","112214",
            "112412","122114","122411","142112","142211","241211","221114","413111","241112","134111",
            "111242","121142","121241","114212","124112","124211","411212","421112","421211","212141",
            "214121","412121","111143","111341","131141","114113","114311","411113","411311","113141",
            "114131","311141","411131","211412","211214","211232","2331112"
        };

        private const int StartB = 104;
        private const int Stop = 106;

        public string GetCode128Svg(string data, int moduleWidth = 2, int barHeight = 80, string? label = null)
        {
            data ??= string.Empty;

            // Build the list of code values: Start B, each character, checksum, Stop.
            var values = new List<int> { StartB };
            foreach (var ch in data)
            {
                var v = ch - 32; // Code 128 subset B maps ASCII 32..126 to values 0..94
                if (v < 0 || v > 94)
                {
                    v = '?' - 32; // substitute unsupported characters
                }
                values.Add(v);
            }

            // Checksum: (startValue + sum(value_i * position_i)) mod 103, position starting at 1.
            long checksum = StartB;
            for (int i = 1; i < values.Count; i++)
            {
                checksum += (long)values[i] * i;
            }
            values.Add((int)(checksum % 103));
            values.Add(Stop);

            // Convert code values to the bar/space module pattern and measure total width.
            var modules = new StringBuilder();
            foreach (var value in values)
            {
                modules.Append(Patterns[value]);
            }

            int totalModules = 0;
            foreach (var c in modules.ToString())
            {
                totalModules += c - '0';
            }

            const int quietZone = 10; // modules of blank space on each side
            int totalWidth = (totalModules + quietZone * 2) * moduleWidth;
            int textHeight = 22;
            int svgHeight = barHeight + textHeight;

            var svg = new StringBuilder();
            svg.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{totalWidth}\" height=\"{svgHeight}\" ")
               .Append($"viewBox=\"0 0 {totalWidth} {svgHeight}\" shape-rendering=\"crispEdges\">");
            svg.Append($"<rect width=\"{totalWidth}\" height=\"{svgHeight}\" fill=\"#ffffff\"/>");

            // Draw bars. Widths alternate bar/space starting with a bar.
            int x = quietZone * moduleWidth;
            bool isBar = true;
            foreach (var c in modules.ToString())
            {
                int w = (c - '0') * moduleWidth;
                if (isBar)
                {
                    svg.Append($"<rect x=\"{x}\" y=\"0\" width=\"{w}\" height=\"{barHeight}\" fill=\"#000000\"/>");
                }
                x += w;
                isBar = !isBar;
            }

            // Human-readable text under the barcode (caption may differ from the encoded data).
            var caption = System.Security.SecurityElement.Escape(label ?? data);
            svg.Append($"<text x=\"{totalWidth / 2}\" y=\"{barHeight + 17}\" text-anchor=\"middle\" ")
               .Append($"font-family=\"monospace\" font-size=\"16\" letter-spacing=\"2\" fill=\"#000000\">{caption}</text>");

            svg.Append("</svg>");
            return svg.ToString();
        }

        public string GetQrCodePngDataUri(string data, int pixelsPerModule = 8)
        {
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(data ?? string.Empty, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(qrData);
            byte[] bytes = pngQr.GetGraphic(pixelsPerModule);
            return "data:image/png;base64," + Convert.ToBase64String(bytes);
        }
    }
}

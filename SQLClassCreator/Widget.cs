using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Xml;

namespace Namespace.Data
{
    public record widget(string? debug, window? window, image? image, text? text)
    {
    }
    public record window(string? title, string? name, int? width, int? height)
    {
    }
    public record image(string? src, string? name, int? hOffset, int? vOffset, string? alignment)
    {
    }
    public record text(string? data, int? size, string? style, string? name, int? hOffset, int? vOffset, string? alignment, string? onMouseUp)
    {
    }
}
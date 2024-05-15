using Org.BouncyCastle.Tls.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace eVR.Reader.Demo.Views
{
    /// <summary>
    /// Interaction logic for XmlView.xaml
    /// </summary>
    public partial class XmlView : UserControl
    {
        #region Depenceny Properties

        /// <summary>
        /// The Xml can be bound to the user control from the xaml
        /// </summary>
        public string Xml
        {
            get { return (string)GetValue(XmlProperty); }
            set { SetValue(XmlProperty, value); }
        }

        public static readonly DependencyProperty XmlProperty =
            DependencyProperty.Register("Xml", typeof(string),
              typeof(XmlView), new UIPropertyMetadata(string.Empty, (d, e) => ((XmlView)d).BindXml()));

        #endregion

        #region Constructor
        public XmlView()
        {
            InitializeComponent();
        }
        #endregion

        #region Private Methods
        private void BindXml()
        {
            if (string.IsNullOrEmpty(Xml)) 
            { 
                return; 
            }
            XDocument xDocument = XDocument.Parse(Xml);
            var items = ParseXml(xDocument.Root!);
            var binding = new Binding
            {
                Source = items
            };
            xmlTree.SetBinding(ItemsControl.ItemsSourceProperty, binding);
        }
        private static List<XmlTag> ParseXml(XElement element)
        {
            var result = new List<XmlTag>();
            if (element.HasElements)
            {
                result.Add(new XmlOpenTag
                {
                    Tag = element.Name.LocalName,
                    Tags = element.Elements().SelectMany(e => ParseXml(e)).ToList(),
                    Attributes = ParseAttributes(element.Attributes())
                });
                result.Add(new XmlCloseTag { Tag = element.Name.LocalName });
            }
            else if (!string.IsNullOrEmpty(element.Value))
            {
                result.Add(new XmlValueTag
                {
                    Tag = element.Name.LocalName,
                    Attributes = ParseAttributes(element.Attributes()),
                    Value = element.Value,
                });
            }
            else
            {
                result.Add(new XmlClosedTag
                {
                    Tag = element.Name.LocalName,
                    Attributes = ParseAttributes(element.Attributes())
                });
            }
            return result;
        }

        private static List<XmlAttribute> ParseAttributes(IEnumerable<XAttribute> attributes)
        {
            if (attributes == null)
            {
                return [];
            }
            return attributes.Select(a => new XmlAttribute
            {
                Name = a.Name.LocalName,
                Value = a.Value
            }).ToList();
        }
        #endregion
    }

    /// <summary>
    /// Helper classes for the presentation of the xml
    /// </summary>
    public abstract class XmlTag
    {
        public string Tag { get; set; } = string.Empty;
    }
    /// <summary>
    /// Closed tag, e.g. &lt;xml attribute="value"/&gt;
    /// </summary>
    public class XmlClosedTag : XmlTag
    {
        public List<XmlAttribute>? Attributes { get; set; }
    }

    /// <summary>
    /// Tag with value, e.g. &lt;xml attribute="value"&gt;value of the tag&lt;/xml&gt;
    /// </summary>
    public class XmlValueTag : XmlClosedTag
    {
        public string Value { get; set; } = string.Empty;
    }
    /// <summary>
    /// Tag that will be followed by child elements, e.g. &lt;xml&gt;...
    /// </summary>
    public class XmlOpenTag : XmlClosedTag
    {
        public List<XmlTag> Tags { get; set; } = [];
    }

    /// <summary>
    /// Tag to close a collection of child elements, e.g.  ...&lt;/xml&gt;
    /// </summary>
    public class XmlCloseTag : XmlTag
    {
    }

    /// <summary>
    /// Representation of an xml attribute, e.g. attribute="value"
    /// </summary>
    public class XmlAttribute
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}

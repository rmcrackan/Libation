using System;
using System.Linq;
using Dinah.Core;

namespace Scraping.Selectors
{
    internal partial class By
    {
        private static string getDescription(string param, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
            => $"{nameof(By)}.{caller}: {param}";

        /// <summary>
        /// Gets a mechanism to find elements by their CSS class.
        /// </summary>
        /// <param name="classNameToFind">The CSS class to find.</param>
        /// <returns>A <see cref="By"/> object the driver can use to find the elements.</returns>
        /// <remarks>If an element has many classes then this will match against each of them.
        /// For example if the value is "one two onone", then the following values for the
        /// className parameter will match: "one" and "two".</remarks>
        public static By ClassName(string classNameToFind)
        {
            if (classNameToFind == null)
                throw new ArgumentNullException(nameof(classNameToFind), "Cannot find elements when the class name is null.");

            return new By(
                getDescription(classNameToFind),
                (context) => context.Node.Descendants().Where(n => n.HasClass(classNameToFind)).ToReadOnlyCollection());
        }

        /// <summary>Gets a mechanism to find elements by their ID.</summary>
        /// <param name="idToFind">The ID to find.</param>
        /// <returns>A <see cref="By"/> object the driver can use to find the elements.</returns>
        public static By Id(string idToFind)
        {
            if (idToFind == null)
                throw new ArgumentNullException(nameof(idToFind), "Cannot find elements when the id is null.");

            return new By(
                getDescription(idToFind),
                (context) => context.Node.Descendants().Where(n => n.Id.EqualsInsensitive(idToFind)).ToReadOnlyCollection());
        }

        /// <summary>Gets a mechanism to find elements by their link text.</summary>
        /// <param name="linkTextToFind">The link text to find.</param>
        /// <returns>A <see cref="By"/> object the driver can use to find the elements.</returns>
        public static By LinkText(string linkTextToFind)
        {
            if (linkTextToFind == null)
                throw new ArgumentNullException(nameof(linkTextToFind), "Cannot find elements when the link text is null.");

            return new By(
                getDescription(linkTextToFind),
                (context) => context.Node.Descendants("a").Where(n => n.InnerText.EqualsInsensitive(linkTextToFind)).ToReadOnlyCollection());
        }

        /// <summary>selenium Name == hap get element with attribute named "name". Gets a mechanism to find elements by their name.</summary>
        /// <param name="nameToFind">The name to find.</param>
        /// <returns>A <see cref="By"/> object the driver can use to find the elements.</returns>
        public static By Name(string nameToFind)
        {
            if (nameToFind == null)
                throw new ArgumentNullException(nameof(nameToFind), "Cannot find elements when the tag name is null.");

            return new By(
                getDescription(nameToFind),
                //.Descendants().Single(n => n.HasAttributes && n.Attributes["name"] != null && n.Attributes["name"].Value.EqualsInsensitive("tdTitle"))
                (context) => context.Node.SelectNodes($".//*[@name='{nameToFind}']").ToReadOnlyCollection());
        }

        /// <summary>selenium TagName == hap Name. Gets a mechanism to find elements by their tag name.</summary>
        /// <param name="tagNameToFind">The tag name to find.</param>
        /// <returns>A <see cref="By"/> object the driver can use to find the elements.</returns>
        public static By TagName(string tagNameToFind)
        {
            if (tagNameToFind == null)
                throw new ArgumentNullException(nameof(tagNameToFind), "Cannot find elements when the tag name is null.");

            return new By(
                getDescription(tagNameToFind),
                (context) => context.Node.Descendants(tagNameToFind).ToReadOnlyCollection());
        }

        /// <summary>
        /// Gets a mechanism to find elements by an XPath query.
        /// When searching within a WebElement using xpath be aware that WebDriver follows standard conventions:
        /// a search prefixed with "//" will search the entire document, not just the children of this current node.
        /// Use ".//" to limit your search to the children of this WebElement.
        /// </summary>
        /// <param name="xpathToFind">The XPath query to use.</param>
        /// <returns>A <see cref="By"/> object the driver can use to find the elements.</returns>
        public static By XPath(string xpathToFind)
        {
            if (xpathToFind == null)
                throw new ArgumentNullException(nameof(xpathToFind), "Cannot find elements when the XPath expression is null.");

            return new By(
                getDescription(xpathToFind),
                (context) => context.Node.SelectNodes(xpathToFind).ToReadOnlyCollection());
        }
    }
}

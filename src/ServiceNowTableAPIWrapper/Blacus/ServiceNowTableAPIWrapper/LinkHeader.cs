using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Parses RFC 5988/8288 pagination links returned in HTTP Link response headers.
    /// </summary>
    internal class LinkHeader
    {
        /// <summary>
        /// Gets or sets the first page link URL.
        /// </summary>
        public string? FirstLink { get; set; }

        /// <summary>
        /// Gets or sets the previous page link URL.
        /// </summary>
        public string? PrevLink { get; set; }

        /// <summary>
        /// Gets or sets the next page link URL.
        /// </summary>
        public string? NextLink { get; set; }

        /// <summary>
        /// Gets or sets the last page link URL.
        /// </summary>
        public string? LastLink { get; set; }

        /// <summary>
        /// Parses a raw HTTP <c>Link</c> header string into a <see cref="LinkHeader"/> object.
        /// </summary>
        /// <param name="linkHeaderStr">Raw HTTP Link header text.</param>
        /// <returns>A populated <see cref="LinkHeader"/> object or null.</returns>
        public static LinkHeader? LinksFromHeader(string? linkHeaderStr)
        {
            if (string.IsNullOrWhiteSpace(linkHeaderStr))
            {
                return null;
            }

            string[] linkStrings = linkHeaderStr.Split(',');
            if (linkStrings == null || !linkStrings.Any())
            {
                return null;
            }

            var linkHeader = new LinkHeader();

            foreach (string linkString in linkStrings)
            {
                var relMatch = Regex.Match(linkString, "(?<=rel=\"?)[a-zA-Z]+(?=\"?)", RegexOptions.IgnoreCase);
                var linkMatch = Regex.Match(linkString, "(?<=<).+?(?=>)", RegexOptions.IgnoreCase);

                if (relMatch.Success && linkMatch.Success)
                {
                    string rel = relMatch.Value.ToUpperInvariant();
                    string link = linkMatch.Value;

                    switch (rel)
                    {
                        case "FIRST":
                            linkHeader.FirstLink = link;
                            break;
                        case "PREV":
                            linkHeader.PrevLink = link;
                            break;
                        case "NEXT":
                            linkHeader.NextLink = link;
                            break;
                        case "LAST":
                            linkHeader.LastLink = link;
                            break;
                    }
                }
            }

            return linkHeader;
        }
    }
}

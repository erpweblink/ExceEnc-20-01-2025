<?xml version="1.0"?>
<doc>
    <assembly>
        <name>Select.HtmlToPdf.NetCore20</name>
    </assembly>
    <members>
        <member name="T:SelectPdf.BeforeCreateNextPageHandler">
            <summary>
            BeforeCreateNextPage event handler
            </summary>
            <param name="bcnpEventArgs">The BeforeCreateNextPage event handler</param>
        </member>
        <member name="T:SelectPdf.BeforeCreateNextPageEventArgs">
            <summary>
            BeforeCreateNextPage event arguments
            </summary>
        </member>
        <member name="P:SelectPdf.BeforeCreateNextPageEventArgs.Cancel">
            <summary>
            Set this property true to cancel automatic creation of next page. 
            This will also end the rendering of the next page.
            </summary>
        </member>
        <member name="T:SelectPdf.BeforeRenderNextPageHandler">
            <summary>
            BeforeRenderNextPage event handler
            </summary>
            <param name="brnpEventArgs">The BeforeRenderNextPage event handler</param>
        </member>
        <member name="T:SelectPdf.BeforeRenderNextPageEventArgs">
            <summary>
            BeforeRenderNextPage event arguments
            </summary>
        </member>
        <member name="M:SelectPdf.BeforeRenderNextPageEventArgs.#ctor(System.Int32,System.Drawing.RectangleF)">
            <summary>
            Constructor
            </summary>
            <param name="pageIndex">The next page index</param>
            <param name="rectangle">The rectangle rendered on the next page</param>
        </member>
        <member name="P:SelectPdf.BeforeRenderNextPageEventArgs.Cancel">
            <summary>
            Set this property true to cancel rendering on next page
            </summary>
        </member>
        <member name="P:SelectPdf.BeforeRenderNextPageEventArgs.Rectangle">
            <summary>
            The rectangle that will be rendered on the next page
            </summary>
        </member>
        <member name="P:SelectPdf.BeforeRenderNextPageEventArgs.PageIndex">
            <summary>
            The index of the next page
            </summary>
        </member>
        <member name="T:SelectPdf.GlobalProperties">
            <summary>
            Global properties for SelectPdf SDK.
            </summary>
        </member>
        <member name="P:SelectPdf.GlobalProperties.HtmlEngineFullPath">
            <summary>
            Gets or sets the full path (including the file name) of the html rendering engine helper file. 
            </summary>
        </member>
        <member name="P:SelectPdf.GlobalProperties.PdfToolsFullPath">
            <summary>
            Gets or sets the full path (including the file name) of the pdf tools engine helper file. 
            </summary>
        </member>
        <member name="P:SelectPdf.GlobalProperties.LFN">
            <summary>
            Internal use only.
            </summary>
        </member>
        <member name="P:SelectPdf.GlobalProperties.ForceDenyLocalFileAccess">
            <summary>
            A flag indicating if local files can be loaded during the conversion. The default value is False and local files can be loaded.
            </summary>
            <remarks>
            This global flag takes priority. If it is set, the corresponding per object flag (DenyLocalFileAccess) is ignored.</remarks>
        </member>
        <member name="P:SelectPdf.GlobalProperties.EnableRestrictedRenderingEngine">
            <summary>
            Enable or disable restricted rendering engine.
            </summary>
            <remarks>
            There are environments, such as Microsoft Azure Web Apps, where GDI calls are restricted and the default rendering engine does not work.
            To enable an alternative rendering engine (with some restrictions - no web fonts for html to pdf conversions, one page PdfHtmlElement objects) set this property to True. 
            The default value for this property is False and the restricted rendering engine is not enabled by default.
            </remarks>
        </member>
        <member name="P:SelectPdf.GlobalProperties.EnableFallbackToRestrictedRenderingEngine">
            <summary>
            Enable or disable fall-back to restricted rendering engine.
            </summary>
            <remarks>
            There are environments, such as Microsoft Azure Web Apps, where GDI calls are restricted and the default rendering engine does not work.
            To enable an alternative rendering engine (with some restrictions - no web fonts for html to pdf conversions, one page PdfHtmlElement objects) 
            set <see cref="P:SelectPdf.GlobalProperties.EnableRestrictedRenderingEngine"/> to True,
            or set this property to True to automatically enable the restricted rendering engine if the main engine fails.
            The default value for this property is True and the restricted rendering engine is activated when main engine fails.
            </remarks>
        </member>
        <member name="T:SelectPdf.ConverterUtils">
            <summary>
            HTML converter utilities
            </summary>
        </member>
        <member name="M:SelectPdf.ConverterUtils.GetHtmlFromURL(System.String)">
            <summary>
            Get the HTML code from the specified URL. Use the autodetermined page encoding
            to create the resulted string object. The default code page is UTF8 if the page
            has no encoding specified
            </summary>
            <param name="url">The URL from where to get the HTML</param>
            <returns>The page HTML string</returns>
        </member>
        <member name="M:SelectPdf.ConverterUtils.GetHtmlFromURL(System.String,System.Text.Encoding)">
            <summary>
            Get the HTML code from the specified URL. Use the specified page encoding
            to create the resulted string object. 
            </summary>
            <param name="url">The URL from where to get the HTML</param>
            <param name="pageEncoding">The encoding used to build the resulted string object</param>
            <returns>The page HTML string</returns>
        </member>
        <member name="M:SelectPdf.ConverterUtils.GetHtmlFromURL(System.Net.HttpWebRequest,System.Text.Encoding)">
            <summary>
            
            </summary>
            <param name="request">The HttpWebRequest object to to make the HTTP request</param>
            <param name="pageEncoding">The encoding used to build the resulted string object</param>
            <returns>The page HTML string</returns>
        </member>
        <member name="T:SelectPdf.HiddenWebElements">
            <summary>
            Helps defining a set of html elements that will not be displayed in the generated pdf document.
            </summary>
        </member>
        <member name="P:SelectPdf.HiddenWebElements.CssSelectors">
            <summary>
            This property is used to define an array containing the selectors of the html elements that will not be displayed in the final pdf document. 
            For example, the selector for all the image elements is "img", the selector for all the elements with the CSS class name 'myclass'
            is "*.myclass" and the selector for the elements with the id 'myid' is "*#myid".
            </summary>
        </member>
        <member name="T:SelectPdf.SecureProtocol">
            <summary>
            Protocol used for secure (HTTPS) connections.
            </summary>
        </member>
        <member name="F:SelectPdf.SecureProtocol.Tls11OrNewer">
            <summary>
            TLS 1.1 or newer. Recommended value.
            </summary>
        </member>
        <member name="F:SelectPdf.SecureProtocol.Tls10">
            <summary>
            TLS 1.0 only.
            </summary>
        </member>
        <member name="F:SelectPdf.SecureProtocol.Ssl3">
            <summary>
            SSL v3 only.
            </summary>
        </member>
        <member name="T:SelectPdf.HtmlToImage">
            <summary>
            Html to Image Converter. This class offers the API needed to create images in various formats from
            a specified web page or html string.
            </summary>
        </member>
        <member name="M:SelectPdf.HtmlToImage.#ctor">
            <summary>
            Creates an html to image converter. Width and height of the web page are automatically detected.
            </summary>
        </member>
        <member name="M:SelectPdf.HtmlToImage.#ctor(System.Int32)">
            <summary>
            Creates an html to image converter. The width of the web page is specified. The height of the web page is automatically detected.
            </summary>
            <param name="webPageWidth">The web page width.</param>
        </member>
        <member name="M:SelectPdf.HtmlToImage.#ctor(System.Int32,System.Int32)">
            <summary>
            Creates an html to image converter for a web page with the specified width and height.
            </summary>
            <param name="webPageWidth">The web page width.</param>
            <param name="webPageHeight">The web page height.</param>
        </member>
        <member name="P:SelectPdf.HtmlToImage.LFN">
            <summary>
            Internal use only.
            </summary>
        </member>
        <member name="P:SelectPdf.HtmlToImage.VisibleWebElementId">
            <summary>
            Use this property to convert only a certain section of the web page, specified by the html element ID.
            </summary>
        </member>
        <member name="P:SelectPdf.HtmlToImage.StartupMode">
            <summary>
            Use this property to specify how the conversion starts.
            </summary>
            <remarks>
            By default this is set to <see cref="F:SelectPdf.HtmlToPdfStartupMode.Automatic"/> and the conversion is started as soon as the page loads (and <see cref="P:SelectPdf.HtmlToImage.MinPageLoadTime"/> elapses). 
            If set to <see cref="F:SelectPdf.HtmlToPdfStartupMode.Manual"/>, the conversion is started only by a javascript call to <c>SelectPdf.startConversion()</c> from within the web page.
            </remarks>
        </member>
        <member name="P:SelectPdf.HtmlToImage.StartupScript">
            <summary>
            Use this property to specify some JavaScript code that will be injected into the page that is converted. 
            </summary>
            <remarks>The JavaScript code specified here will run before any other script on the page.</remarks>
        </member>
        <member name="P:SelectPdf.HtmlToImage.WebPageWidth">
            <summary>
            Gets or sets the web page width.
            </summary>
        </member>
        <member name="P:SelectPdf.HtmlToImage.WebPageHeight">
            <summary>
            Gets or sets the web page height. If the width was not set, this property has no effect.
            </summary>
        </member>
        <member name="P:SelectPdf.HtmlToImage.WebPageFixedSize">
            <summary>Control